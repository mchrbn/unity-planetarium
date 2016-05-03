using UnityEngine;
using System;

public static class CelestialCoordinates{

	/// <summary>
	/// Return an angle in the range of 0 -> 2PI Radians
	/// </summary>
	/// <returns>Angle in the range 0 - 2PI</returns>
	/// <param name="_angle">Angle to convert</param>
	private static double Mod2Pi(double _angle){
		double b = _angle / (2.0f * Mathf.PI);
		double a = (2.0f * Mathf.PI) * (b - Mathf.Floor((float)b));  
		if (a < 0) a = (2.0f * Mathf.PI) + a;
		return a;
	}

	/// <summary>
	/// Trues the anomaly.
	/// To find what's this function is about...didn't understand it
	/// </summary>
	/// <returns>The anomaly</returns>
	/// <param name="M">M.</param>
	/// <param name="e">E.</param>
	private static double TrueAnomaly(double M, double e){
		double V;

		// initial approximation of eccentric anomaly
		var E = M + e*Math.Sin(M)*(1.0f + e*Math.Cos(M));

		// convert eccentric anomaly to true anomaly
		V = 2f * Math.Atan(Math.Sqrt((1f + e)/(1f - e))*Math.Tan(0.5f*E));

		if (V < 0) V = V + (2f * Mathf.PI);

		return V;
	}

	/// <summary>
	/// Calculates the horizontal coordinates of the moon
	/// Source: https://github.com/mourner/suncalc/blob/master/suncalc.js
	/// </summary>
	/// <returns>The horizontal coordinatesof the moon</returns>
	/// <param name="_longitude">User longitude in degree</param>
	/// <param name="_latitude">User latitude in degree</param>
	public static Vector3 CalculateHorizontalCoordinatesMoon(double _longitude, double _latitude){

		//Convert the latitude to radians
		_latitude *= Mathf.Deg2Rad;

		double lw = Mathf.Deg2Rad * _longitude * -1;
		DateTime epoch = new DateTime(2000, 1, 1, 12, 0, 0);
		TimeSpan j2000TS = DateTime.UtcNow - epoch;
		double j2000 = j2000TS.TotalDays;

		double L = Mathf.Deg2Rad * (218.316f + 13.176396f * j2000), // ecliptic longitude
		M = Mathf.Deg2Rad * (134.963f + 13.064993f * j2000), // mean anomaly
		F = Mathf.Deg2Rad * (93.272f + 13.229350f * j2000),  // mean distance

		l  = L + Mathf.Deg2Rad * 6.289f * Math.Sin(M), // longitude
		b  = Mathf.Deg2Rad * 5.128f * Math.Sin(F),     // latitude
		dt = 385001f - 20905f * Math.Cos(M);  // distance to the moon in km

		//Calculate the Right ascension and declinaison of the moon
		double e = Mathf.Deg2Rad * 23.4397f;
		double ra = Math.Atan2(Math.Sin(l) * Math.Cos(e) - Math.Tan(b) * Math.Sin(e), Math.Cos(l));
		double dec = Math.Asin(Math.Sin(b) * Math.Cos(e) + Math.Cos(b) * Math.Sin(e) * Math.Sin(l));

		//Sidereal time
		double HA = (Mathf.Deg2Rad * (280.16f + 360.9856235f * j2000) - lw) - ra;

		//Compute Altitude and Azimuth (RAD)
		double altitude = Math.Asin(Math.Sin(dec) * Math.Sin(_latitude) + Math.Cos(dec) * Math.Cos(_latitude) * Math.Cos(HA));
		double azimuth = Math.Acos((Math.Sin(dec) - Math.Sin(altitude) * Math.Sin(_latitude)) / (Math.Cos(altitude)*Math.Cos(_latitude)));

		//Convert both to degree
		altitude *= Mathf.Rad2Deg;
		azimuth *= Mathf.Rad2Deg;

		if (Math.Sin (HA) > 0f)
			azimuth = 360f - azimuth;

		return new Vector3 ((float)altitude, (float)azimuth, (float)dt/500000f);
	}


	/// <summary>
	/// Calculate the altitude and azimuth of all planets of our solar system / including sun.
	/// Source: http://www.abecedarical.com/javascript/script_planet_orbits.html
	/// </summary>
	/// <returns>The altitude azimuth (in degree) as well as the distance of where the star is located</returns>
	/// <param name="_longitude">User longitude in degree</param>
	/// <param name="_latitude">User latitude in degree</param> 
	/// <param name="_name">The planet's name</param> 
	public static Vector3 CalculateHorizontalCoordinatesPlanets(double _longitude, double _latitude, string _name){

		//Convert the latitude to radians
		_latitude *= Mathf.Deg2Rad;

		Vector3 coordinates = new Vector3(0,0,0);

		//1. Days elapsed since J2000 (1st january 2000 at 12:00)
		DateTime epoch = new DateTime(2000, 1, 1, 12, 0, 0);
		TimeSpan j2000TS = DateTime.UtcNow - epoch;
		double j2000 = j2000TS.TotalDays;

		//2. Centuries since J2000
		double cJ2000 = j2000 / 36525.0f;

		//3. Depending on the planet, sets its parameters that we use later to calculate Alt + Az
		double inclination = 0f;	//angle between the plane of the ecliptic (the plane of Earth's orbit about the Sun) and the plane of the planets orbit
		double longNode = 0f;		//longitude of ascending node (degrees), the position in the orbit where the elliptical path of the planet passes through the plane of the ecliptic, from below the plane to above the plane
		double longPeri = 0f;		//longitude of perihelion (degrees), the position in the orbit where the planet is closest to the Sun
		double meanDist = 0f;		//mean distance (AU), the value of the semi-major axis of the orbit (AU - Astronomical Unit - average Sun to Earth distance)
		double eccentricity = 0f;	//eccentricity of the ellipse which describes the orbit (dimensionless)
		double meanLong = 0f;		//mean longitude (degrees), the position of the planet in the orbit

		//In all cases save these variable for the Earth as well...which uses the same equation as the Sun!
		double inclinationE = (0.00005f - 46.94f * cJ2000 / 3600.0f) * Mathf.Deg2Rad;;
		double longNodeE = (-11.26064f - 18228.25f * cJ2000 / 3600.0f) * Mathf.Deg2Rad;;
		double longPeriE = (102.94719f + 1198.28f * cJ2000 / 3600.0f) * Mathf.Deg2Rad;
		double meanDistE = 1.00000011f - 0.00000005f * cJ2000;
		double eccenctricityE = 0.01671022f - 0.00003804f * cJ2000;
		double meanLongE = Mod2Pi ((100.46435f + 129597740.63f * cJ2000 / 3600.0f) * Mathf.Deg2Rad);

		switch(_name){
		case "Sun":
			inclination = inclinationE;
			longNode = longNodeE;
			longPeri = longPeriE;
			meanDist = meanDistE;
			eccentricity = eccenctricityE;
			meanLong = meanLongE;
			break;
		case "Mercury":
			inclination = (7.00487f - 23.51f * cJ2000 / 3600f) * Mathf.Deg2Rad;
			longNode = (48.33167f - 446.30f * cJ2000 / 3600f) * Mathf.Deg2Rad;
			longPeri = (77.45645f + 573.57f * cJ2000 / 3600f) * Mathf.Deg2Rad;
			meanDist = 0.38709893f + 0.00000066f * cJ2000;
			eccentricity = 0.20563069f + 0.00002527f * cJ2000;
			meanLong = Mod2Pi ((252.25084f + 538101628.29f * cJ2000 / 3600.0f) * Mathf.Deg2Rad);
			break;
		case "Venus":
			inclination = (3.39471f -   2.86f * cJ2000 / 3600.0f) * Mathf.Deg2Rad;
			longNode = (76.68069f - 996.89f * cJ2000 / 3600f) * Mathf.Deg2Rad;
			longPeri = (131.53298f - 108.80f * cJ2000 / 3600f) * Mathf.Deg2Rad;
			meanDist = 0.72333199f + 0.00000092f * cJ2000;
			eccentricity = 0.00677323f - 0.00004938f * cJ2000;
			meanLong = Mod2Pi ((181.97973f + 210664136.06f * cJ2000 / 3600f) * Mathf.Deg2Rad);
			break;
		case "Mars":
			inclination = (1.85061f - 25.47f * cJ2000 / 3600f) * Mathf.Deg2Rad;
			longNode = (49.57854f - 1020.19f * cJ2000 / 3600f) * Mathf.Deg2Rad;
			longPeri = (336.04084f + 1560.78f * cJ2000 / 3600f) * Mathf.Deg2Rad;
			meanDist = 1.52366231f - 0.00007221f * cJ2000;
			eccentricity = 0.09341233f + 0.00011902f * cJ2000;
			meanLong = Mod2Pi ((355.45332f + 68905103.78f * cJ2000 / 3600f) * Mathf.Deg2Rad);
			break;
		case "Jupiter":
			inclination = (1.30530f - 4.15f * cJ2000 / 3600f) * Mathf.Deg2Rad;
			longNode = (100.55615f + 1217.17f * cJ2000 / 3600f) * Mathf.Deg2Rad;
			longPeri = (14.75385f +  839.93f * cJ2000 / 3600f) * Mathf.Deg2Rad;
			meanDist = 5.20336301f + 0.00060737f * cJ2000;
			eccentricity = 0.04839266f - 0.00012880f * cJ2000;
			meanLong = Mod2Pi ((34.40438f + 10925078.35f * cJ2000 / 3600f) * Mathf.Deg2Rad);
			break;
		case "Saturn":
			inclination = (2.48446f + 6.11f * cJ2000 / 3600f) * Mathf.Deg2Rad;
			longNode = (113.71504f - 1591.05f * cJ2000 / 3600f) * Mathf.Deg2Rad;
			longPeri = (92.43194f - 1948.89f * cJ2000 / 3600f) * Mathf.Deg2Rad;
			meanDist = 9.53707032f - 0.00301530f * cJ2000;
			eccentricity = 0.05415060f - 0.00036762f * cJ2000;
			meanLong = Mod2Pi ((49.94432f + 4401052.95f * cJ2000/3600f) * Mathf.Deg2Rad);
			break;
		case "Uranus":
			inclination = (0.76986f - 2.09f * cJ2000 / 3600f) * Mathf.Deg2Rad;
			longNode = (74.22988f - 1681.40f * cJ2000 / 3600f) * Mathf.Deg2Rad;
			longPeri = (170.96424f  + 1312.56f * cJ2000 / 3600f) * Mathf.Deg2Rad;
			meanDist = 19.19126393f + 0.00152025f * cJ2000;
			eccentricity = 0.04716771f - 0.00019150f * cJ2000;
			meanLong = Mod2Pi ((313.23218f + 1542547.79f * cJ2000 / 3600f) * Mathf.Deg2Rad);
			break;
		case "Neptune":
			inclination = (1.76917f - 3.64f * cJ2000 / 3600f) * Mathf.Deg2Rad;
			longNode = (131.72169f - 151.25f * cJ2000 / 3600f) * Mathf.Deg2Rad;
			longPeri = (44.97135f - 844.43f * cJ2000 / 3600f) * Mathf.Deg2Rad;
			meanDist = 30.06896348f - 0.00125196f * cJ2000;
			eccentricity = 0.00858587f + 0.00002510f * cJ2000;
			meanLong = Mod2Pi ((304.88003f + 786449.21f * cJ2000 / 3600f) * Mathf.Deg2Rad);
			break;
		case "Pluto":
			inclination = (17.14175f + 11.07f * cJ2000 / 3600f) * Mathf.Deg2Rad;
			longNode = (110.30347f - 37.33f * cJ2000 / 3600f) * Mathf.Deg2Rad;
			longPeri = (224.06676f - 132.25f * cJ2000 / 3600f) * Mathf.Deg2Rad;
			meanDist = 39.48168677f - 0.00076912f * cJ2000;
			eccentricity = 0.24880766f + 0.00006465f * cJ2000;
			meanLong = Mod2Pi ((238.92881f + 522747.90f * cJ2000 / 3600f) * Mathf.Deg2Rad);
			break;
		}

		//Position of Earth in its orbit
		double me = Mod2Pi(meanLongE - longPeriE);
		double ve = TrueAnomaly (me, eccenctricityE);
		double pEarthOrbit = meanDistE*(1-eccenctricityE*eccenctricityE)/(1 + eccenctricityE*Math.Cos(ve));

		//Heliocentric rectangular coordinates of Earth
		double xe = pEarthOrbit * Math.Cos(ve + longPeriE);
		double ye = pEarthOrbit * Math.Sin(ve + longPeriE);
		double ze = 0.0f;

		//Position of planet in its orbit
		double mp = Mod2Pi(meanLong - longPeri);
		double vp = TrueAnomaly(mp, eccentricity);
		double pPlanetOrbit = meanDist * (1 - eccentricity * eccentricity) / (1 + eccentricity * Math.Cos (vp));

		//Heliocentric rectangular coordinates of planets
		double xh = pPlanetOrbit * (Math.Cos(longNode) * Math.Cos(vp + longPeri - longNode) - Math.Sin(longNode) * Math.Sin(vp + longPeri - longNode) * Math.Cos(inclination));
		double yh = pPlanetOrbit * (Math.Sin(longNode) * Math.Cos(vp + longPeri - longNode) + Math.Cos(longNode) * Math.Sin(vp + longPeri - longNode) * Math.Cos(inclination));
		double zh = pPlanetOrbit * (Math.Sin (vp + longPeri - longNode) * Math.Sin (inclination));

		//If Sun set the coordinates to 0
		if (_name == "Sun") {
			xh = 0f;
			yh = 0f;
			zh = 0f;
		}

		//Convert to geocentric rectangular coordinates
		double xg = xh - xe;
		double yg = yh - ye;
		double zg = zh - ze;

		// rotate around x axis from ecliptic to equatorial coords
		double ecl = 23.439281f*Mathf.Deg2Rad;
		double xeq = xg;
		double yeq = yg * Math.Cos(ecl) - zg * Math.Sin(ecl);
		double zeq = yg * Math.Sin(ecl) + zg * Math.Cos(ecl);

		// find the RA and DEC from the rectangular equatorial coords
		double ra = Mod2Pi(Math.Atan2(yeq, xeq))*Mathf.Rad2Deg; 
		double dec = Math.Atan(zeq/Math.Sqrt(xeq * xeq + yeq * yeq))*Mathf.Rad2Deg;
		double distance = Math.Sqrt(xeq * xeq + yeq * yeq + zeq * zeq);


		//Now that we have the Right Ascension and the Declinaison of the planet, we can get the Altitude and Azimuth!
		//compute hour angle in degrees
		// mean sidereal time
		double MST = 280.46061837f + 360.98564736629f * j2000 + 0.000387933f * cJ2000 * cJ2000 - cJ2000 * cJ2000 * cJ2000 / 38710000f + _longitude;

		if (MST > 0.0f){
			while (MST > 360.0f)
				MST -= 360.0f;
		}
		else{
			while (MST < 0.0f)
				MST = MST + 360.0;
		}

		//Compute hour angle in degrees
		double HA = MST - ra;
		if (HA < 0) HA = HA + 360;

		//Convert everything to radians
		HA *= Mathf.Deg2Rad;
		dec *= Mathf.Deg2Rad;
		//_latitude *= Mathf.Deg2Rad;

		//Compute Altitude and Azimuth (RAD)
		double altitude = Math.Asin(Math.Sin(dec) * Math.Sin(_latitude) + Math.Cos(dec) * Math.Cos(_latitude) * Math.Cos(HA));
		double azimuth = Math.Acos((Math.Sin(dec) - Math.Sin(altitude) * Math.Sin(_latitude)) / (Math.Cos(altitude)*Math.Cos(_latitude)));

		//Convert both to degree
		altitude *= Mathf.Rad2Deg;
		azimuth *= Mathf.Rad2Deg;

		if (Math.Sin (HA) > 0f)
			azimuth = 360f - azimuth;

		coordinates.x = (float) altitude;
		coordinates.y = (float) azimuth;
		coordinates.z = (float) distance;

		return coordinates;
	}

	/// <summary>
	/// Calculate the Horizontal Coordinates from a star's Celestial Coordinates.
	/// Take into account user time + date + location
	/// </summary>
	/// <returns>The altitude and azimuth (in degree) of where the star is located</returns>
	/// <param name="_longitude">User longitude in degree</param>
	/// <param name="_latitude">User latitude in degree</param> 
	/// <param name="_ra">Right Ascension (in degree)</param>
	/// <param name="_dec">Declinaison (in degree)</param> 
	public static Vector2 CalculateHorizontalCoordinatesStar(double _longitude, double _latitude, float _ra, float _dec){

		//Convert the latitude to radians
		_latitude *= Mathf.Deg2Rad;

		Vector2 coordinates = new Vector2 (0, 0);

		//The right ascension is already in degree in database
		//the declinaison not so convert it to degree as well
		double ra = _ra;
		double dec = _dec;

		//1. Days elapsed since J2000 (1st january 2000 at 12:00)
		DateTime epoch = new DateTime(2000, 1, 1, 12, 0, 0);
		TimeSpan j2000TS = DateTime.UtcNow - epoch;
		double j2000 = j2000TS.TotalDays;

		//2. UT to Decimals
		//Note: time values has to be sent from telescope
		double curTimeUTC = DateTime.UtcNow.TimeOfDay.TotalHours;

		//3. Get Long East = + / West = -
		double longitude = _longitude;
		double latitude = _latitude;

		//4. Local Sidereal Time (LST)
		//The sidereal time is measured by the rotation of the Earth, with respect to the stars (rather than relative to the Sun).
		//Local sidereal time is the right ascension (RA, an equatorial coordinate) of a star on the observers meridian.
		//In other words, he sidereal time is a direct indication of whether a celestial object of known right ascension is observable at that instant.
		double LST = 100.46f + 0.985647f *  j2000 + longitude + curTimeUTC * 15.0f;

		//4a. As RA and DEC are in degree, we convert this one in degrees as well
		int modulo = (int) Math.Floor(LST) / 360;
		LST = (LST - (360 * modulo)) / 15.0f;

		//5. Local Hour Angle
		//If negative, add 360
		double HA = LST - ra;
		if (HA < 0.0f)
			HA += 360.0f;
		HA *= Math.PI / 12.0f;

		//7. Calculate Altitute
		double altitude = Math.Asin(Math.Sin(dec)*Math.Sin(latitude)+Math.Cos(dec)*Math.Cos(latitude)*Math.Cos(HA));
		double altitudeDeg = altitude * Mathf.Rad2Deg;

		//8. Calculate Azimuth
		double azimuth = Math.Acos((Math.Sin(dec) - Math.Sin(altitude) * Math.Sin(latitude)) / (Math.Cos(altitude) * Math.Cos(latitude)));
		double azimuthDeg = azimuth * Mathf.Rad2Deg;


		if (Math.Sin(HA) > 0.0f)
			azimuthDeg = 360.0f - azimuthDeg;

		coordinates.x = (float) altitudeDeg;
		coordinates.y = (float) azimuthDeg;

		return coordinates;
	}
}