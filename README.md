# Unity Planetarium Generator

![Demo 01](/img/demo_01.gif)

C# / Unity class for an accurate (~ -1°/+1°) generation of our universe (stars, planets, moon and the sun) depending on a given location (lat/long) and time.

The main script is *CelestialCoordinates.cs* - you can use it in your own project.

The script *CelestialManager.cs* is an example on how you can use the above script to generate a skymap corresponding to your location and time.

Note that we calculate the position of every objects from a given location on Earth, therefore planets will rotate around the Earth and not from the Sun.

![Demo 02](/img/demo_02.gif)

## How it works

The class returns the horizontal coordinates (azimuth + altitude), in degrees, of a celestial object, by taking into account the given location and the current time.

For planets (Moon and Sun included), the methods return a Vector3 where **x = altitude (in °), y = azimuth (in °) and z = distance (in AU)** from Earth to the celestial object.
For stars, it returns a Vector2 with altitude and azimuth; the distance should be provided by the database you are using (i.e.: [HYG](https://github.com/astronexus/HYG-Database))

For the stars, you will need its right ascension (RA) and declination (DEC). [The HYG database](https://github.com/astronexus/HYG-Database) contains the RA and DEC of almost 120'000 stars.

Assuming that you want to get the coordinates of a celestial object above Beijing (Lat: 39.9 / Long: 116.4), for the current time:


```
// Get coordinates of the Moon when located in Beijing
// moon.x = altitude, moon.y = azimuth, moon.z = distance (in AU) from Earth to the Moon
Vector3 moon = CelestialCoordinates.CalculateHorizontalCoordinatesMoon(116.4f, 39.9f, DateTime.UtcNow);

// Get coordinates of Neptune when located in Beijing
Vector3 neptune = CelestialCoordinates.CalculateHorizontalCoordinatesPlanets(116.4f, 39.9f, Planet.NEPTUNE, DateTime.UtcNow);

//Get coordinates of Polaris (RA = 2.52975 / DEC = 89.264109) when located in Beijing
Vector2 polaris = CelestialCoordinates.CalculateHorizontalCoordinatesStar(116.4f, 39.9f, 2.52975f, 89.264109f, DateTime.UtcNow);

```

### From Altitude/Azimuth to Unity World Space

To convert your altitude and azimuth to your Unity world space, do the following:

```
Vector3 moon = CelestialCoordinates.CalculateHorizontalCoordinatesMoon(116.4f, 39.9f, DateTime.UtcNow);

Vector3 position = Quaternion.Euler(-moon.x, moon.y, 0) * new Vector3(0, 0, moon.z);
moonGO.transform.position = position;
```

You can also refer to the provided example script *CelestialManager.cs* for more information on how to use the results provided by CelestialCoordinates.

---

MIT License

Copyright (c) 2020 Matthieu Cherubini

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

