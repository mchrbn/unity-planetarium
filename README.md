# CelestialCoordinates

C# class for calculating the "exact" position of any stars, planets from our solar system and moon + sun at a given location (lat/long) and current time.

The class was developed for a Unity project so you would have to change few things if you do not want to use this class with Unity (typically Mathf. stuff).

## How it works

The class returns the Horizontal Coordinates (Azimuth + Altitude) in degrees of a celestial object by taking into account the given location and the current time.

There are three methods:   
    1) Getting the moon horizontal coordinates    
    2) Getting the planets (including the Sun) horizontal coordinates    
    3) Getting the stars horizontal coordinates    

For the moon and the planets, it's pretty straighforward. Just put the planet's name in parameter.

For the stars, you will need its right ascension and declinaison. The HYG database (http://www.astronexus.com/hyg) contains the RA and DEC of almost 120'000 stars.

## Credits

1) The Moon Horizontal Coordinates was written by converting this JS code https://github.com/mourner/suncalc/blob/master/suncalc.js     
2) The Planets Horizontal Coordinates was written by converting this JS code
http://www.abecedarical.com/javascript/script_planet_orbits.html     
3) http://www.stargazing.net/kepler/altaz.html for laying out the logic to get the Horizontal Coordinates of a star depending on its RA and DEC.    


