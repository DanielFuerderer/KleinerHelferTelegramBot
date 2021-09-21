using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Data.Entities;
using IoFile = System.IO.File;
using IoDirectory = System.IO.Directory;

namespace Data
{
  internal class Program
  {
    private static void Main(string[] args)
    {
      var dayCareCentersLeonbergText = @"Clara-Grunwald-Kindergarten
Elly-Heuss-Knapp-Kindergarten
Eric-Carle-Kinderhaus Gebersheim
Ezach-Kindergarten
Fröbel-Kindergarten
Halden-Kinderhaus
Johannes-Kindergarten
Kindergarten Eltingen
Kindergarten Mammutzahn
Kindergarten Regenbogen
Kindergarten St. Michael
Kinderhaus Ezach
Kinderhaus Kunterbunt
Kinderhaus Mozartstraße
Kinderhaus Nord
Kinderhaus Spitalhof
Kinderhaus Stadtpark
Kinderhaus Warmbronn
Kükennest
Leo-Kids e.V.
Ludwig-Wolker-Kinderhaus
Martha-Johanna-Haus
Oberlin-Haus
Schopfloch-Kindergarten
Schulkindergarten Rasselbande für entwicklungsverzögerte und geistig behinderte Kinder
Tages- und Pflegemutter e.V. Leonberg
Wald- und Tierkindergarten Seehaus e.V.
Waldkindergarten Wurzelkinder Leonberg e.V.
Waldorf-Kindergarten
Wichern-Kindergarten
Windelflitzer e.V.";

      var dayCareCentersLeonberg =
        dayCareCentersLeonbergText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

      var dayCareCenters = dayCareCentersLeonberg.Select(n => new Institution(Guid.NewGuid().ToString())
      {
        Name = n,
        CommunityId = 71229
      });


      var lkBbcommunites = @"71032 Böblingen
71083 Herrenberg
71088 Holzgerlingen
71229 Leonberg
71272 Renningen
71063 Sindelfingen
71111 Waldenbuch
71263 Weil der Stadt
71134 Aidlingen
71155 Altdorf
71149 Bondorf
75392 Deckenpfronn
71139 Ehningen
71116 Gärtringen
71126 Gäufelden
71120 Grafenau
71157 Hildrizhausen
71131 Jettingen
71106 Magstadt
71159 Mötzingen
71154 Nufringen
71277 Rutesheim
71101 Schönaich
71144 Steinenbronn
71093 Weil im Schönbuch
71287 Weissach";

      var communities = new List<Community>();

      foreach (var line in lkBbcommunites.Split(Environment.NewLine))
      {
        var lineEntries = line.Split(' ');
        var name = string.Join(' ', lineEntries.Skip(1));

        communities.Add(new Community(int.Parse(lineEntries[0]))
        {
          Name = name
        });
      }

      var lkBoeblingen = new County
      {
        Name = "Böblingen",
        CommunityIds = communities.Select(c => c.Id)
      };

      var file = new CommunityFile
      {
        Counties = new[] { lkBoeblingen },
        Communities = communities,
        Institutions = dayCareCenters
      };

      var data = JsonSerializer.Serialize(file);

      IoDirectory.CreateDirectory("Data");

      IoFile.WriteAllText(@"Data\data.json", data);
    }
  }
}