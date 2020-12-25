// Unity Planetarium
// https://github.com/mchrbn/unity-planetarium-generator

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public struct PlanetObject{
    public GameObject gameObject;
    public PlanetName name;
}

public struct StarObject{
    public GameObject gameObject;
    public string name;
    public float ra;
    public float dec;
}

public class CelestialManager : MonoBehaviour
{
    public float latitude;                    // Your latitude
    public float longitude;                  // Your longitude
    public Vector2 planetsMinMaxRadius;     // From which distance to which distance we place the planets
    public float timeSpeedHours = 5f;     //Speed at which the planets move around Earth
    public int timeGMTOffset = 0;         //GMT offset...in this example we have the lat/long of Beijing so GMT should be set to 8
                                          //this is just for having an accurate text date
    public int numberOfDaysFromNow = 0;   //If you want to start from another than today

    public Text dateText;
    //Prefabs for celestial objects
    public GameObject moonPrefab;
    public GameObject sunPrefab;
    public GameObject marsPrefab;
    public GameObject jupiterPrefab;
    public GameObject mercuryPrefab;
    public GameObject neptunePrefab;
    public GameObject plutoPrefab;
    public GameObject saturnPrefab;
    public GameObject uranusPrefab;
    public GameObject venusPrefab;
    public GameObject starPrefab;


    private List<PlanetObject> planets;
    private List<StarObject> stars;
    private DateTime currentDate;
    private TextAsset starsDatabase;
    private GameObject starsParent;
    private GameObject polaris;

    void Start()
    {
        starsParent = GameObject.Find("Stars");
        starsDatabase = Resources.Load<TextAsset>("hyg_small") as TextAsset;
        currentDate = DateTime.UtcNow.AddDays(numberOfDaysFromNow);
        SetupPlanets();
        SetupStars();
    }

    void Update()
    {
        //Update time
        DateTime pastDate = currentDate;
        currentDate = currentDate.AddHours(timeSpeedHours * Time.deltaTime);
        dateText.text = currentDate.AddHours(8).ToString();
        
        //Update planets and moon position individually
        foreach(PlanetObject planet in planets){
            if(planet.name == PlanetName.MOON){
                Vector3 moonAltAz = CelestialCoordinates.CalculateHorizontalCoordinatesMoon(longitude, latitude, currentDate);
                planet.gameObject.transform.position = GetPlanetsGamePositionFromAltAz(moonAltAz);
            }
            else{
                Vector3 coAltAz = CelestialCoordinates.CalculateHorizontalCoordinatesPlanets(longitude, latitude, planet.name, currentDate);
                planet.gameObject.transform.position = GetPlanetsGamePositionFromAltAz(coAltAz);
            }
        }

        //It would be too expensive to rotate each stars individually like we did for the planets, instead rotate all the stars together
        //We just have to rotate our parent gameobject that contains all the stars with polaris as a pivot point
        //For rotation angle -> earth does a full rotation in 24hours so 15 degree per hour
        float ellapsedH = (float)(currentDate - pastDate).TotalHours;
        starsParent.transform.Rotate(polaris.transform.position, 15f * ellapsedH);
        
    }

    void SetupPlanets(){
        planets = new List<PlanetObject>();

        InstantiatePlanet(moonPrefab, PlanetName.MOON);
        InstantiatePlanet(sunPrefab, PlanetName.SUN);
        InstantiatePlanet(marsPrefab, PlanetName.MARS);
        InstantiatePlanet(mercuryPrefab, PlanetName.MERCURY);
        InstantiatePlanet(venusPrefab, PlanetName.VENUS);
        InstantiatePlanet(jupiterPrefab, PlanetName.JUPITER);
        InstantiatePlanet(saturnPrefab, PlanetName.SATURN);
        InstantiatePlanet(uranusPrefab, PlanetName.URANUS);
        InstantiatePlanet(neptunePrefab, PlanetName.NEPTUNE);
        InstantiatePlanet(plutoPrefab, PlanetName.PLUTO);
    }

    void SetupStars(){
        stars = new List<StarObject>();

        string[] starsLine = starsDatabase.text.Split('\n');

        foreach(string str in starsLine){
            string[] data = str.Split(',');
            //7 = ra, 8 = declination, 13 = magnitude (apparent brightness of the star), 6 = proper name
            InstantiateStar(float.Parse(data[7]), float.Parse(data[8]), float.Parse(data[13]), data[6]);
        }
    }

    void InstantiatePlanet(GameObject _prefab, PlanetName _name){
        PlanetObject co;
        Vector3 altAzDist = Vector3.zero;
        GameObject planetsParent = GameObject.Find("Planets");
        
        //Get the altitude (.x), azimuth (.y) and distance (.z) from Earth
        if(_name == PlanetName.MOON)
            altAzDist = CelestialCoordinates.CalculateHorizontalCoordinatesMoon(longitude, latitude, currentDate);
        else
            altAzDist = CelestialCoordinates.CalculateHorizontalCoordinatesPlanets(longitude, latitude, _name, currentDate);
        
        //Instantiate the corresponding prefab + convert alt/az/dist to game scene
        co.gameObject = Instantiate(_prefab, GetPlanetsGamePositionFromAltAz(altAzDist), Quaternion.identity);
        co.gameObject.name = _name.ToString();
        co.name = _name;
        co.gameObject.transform.SetParent(planetsParent.transform);
        planets.Add(co);
    }

    void InstantiateStar(float _ra, float _dec, float _mag, string _name){
        StarObject so;
        
        //Get the altitude and azimuth of the star
        Vector2 altAz =  CelestialCoordinates.CalculateHorizontalCoordinatesStar(longitude, latitude, _ra, _dec, currentDate);

        //Instantiate the gameobject
        Vector3 pos = Quaternion.Euler(-altAz.x, altAz.y, 0) * new Vector3(0, 0, 1000);

        //Set properties to the struct
        so.gameObject = Instantiate(starPrefab, pos, Quaternion.identity);
        so.name = _name;
        so.ra = _ra;
        so.dec = _dec;
        if(_name != "") so.gameObject.name = _name;
        so.gameObject.transform.SetParent(starsParent.transform);

        //Change the luminosity of the material according to the star's magnitude
        //The lower a magnitude is, the most intense the luminosity is
        Material mat = so.gameObject.GetComponent<Renderer>().material;
        mat.SetColor("_EmissionColor", Color.white * Mathf.Max(7 - _mag, 1));

        stars.Add(so);

        //Save polaris for later - we need to rotate our universe around it
        if(_name == "Polaris")
            polaris = so.gameObject;
    }

    Vector3 GetPlanetsGamePositionFromAltAz(Vector3 _altAz){
        //Remap AU to our scene distance
        float distance = Map(_altAz.z, 0, 40, 2f, 50f);
        Vector3 altAzDist = Quaternion.Euler(-_altAz.x, _altAz.y, 0) * new Vector3(0, 0, distance);
        return altAzDist;
    }

    float Map(float s, float a1, float a2, float b1, float b2){
		if (a1 == a2)
			return b1;
        
		return b1 + (s-a1)*(b2-b1)/(a2-a1);
	}
}
