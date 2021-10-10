using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstructionLine.CodingChallenge
{
    public class SearchEngine
    {
        private readonly List<Shirt> _shirts;

        private readonly Dictionary<KeyDefinition, List<Shirt>> _shirtsMap = new Dictionary<KeyDefinition, List<Shirt>>();

        public SearchEngine(List<Shirt> shirts)
        {
            _shirts = shirts;

            InitializeData();
        }

        public SearchResults Search(SearchOptions options)
        {
            //This approach uses Dictionary mapping, which has O(1) complexity. I got average 3ms for 50,000 items and 10ms for 5,000,000.
            var result = new List<Shirt>();
            var colors = new Dictionary<Guid, int>();
            var sizes = new Dictionary<Guid, int>();
            var sizeKeyParts = options.Sizes.Count > 0 ? options.Sizes.Select(x => x.Id) : Size.All.Select(s => s.Id);
            var colorKeyParts = options.Colors.Count > 0 ? options.Colors.Select(x => x.Id) : Color.All.Select(c => c.Id);
            
            foreach (var sizeKeyPart in sizeKeyParts)
            {
                foreach (var colorKeyPart in colorKeyParts)
                {
                    var key = new KeyDefinition { Size = sizeKeyPart, Color = colorKeyPart };
                    if (_shirtsMap.TryGetValue(key, out List<Shirt> value))
                    {
                        result.AddRange(value);

                        colors.TryGetValue(key.Color, out int colorsCount);
                        colors[key.Color] = colorsCount + value.Count;

                        sizes.TryGetValue(key.Size, out int sizesCount);
                        sizes[key.Size] = sizesCount + value.Count;
                    }
                }
            }

            return new SearchResults
            {
                Shirts = result,
                SizeCounts = Size.All.Select(s => new SizeCount { Size = s, Count = sizes.TryGetValue(s.Id, out int sCount) ? sCount : 0 }).ToList(),
                ColorCounts = Color.All.Select(c => new ColorCount { Color = c, Count = colors.TryGetValue(c.Id, out int cCount) ? cCount : 0 }).ToList(),
            };

            //Having even this simple Linq we can be within 1sec for 50000 items.
            //But of course it will be very bad for more items. Because complexity is O(N).
            //var result = _shirts.Where(x => (options.Colors.Contains(x.Color) || !options.Colors.Any()) && (options.Sizes.Contains(x.Size) || !options.Sizes.Any())).ToList();

            //return new SearchResults
            //{
            //    Shirts = result,
            //    SizeCounts = Size.All.Select(s => new SizeCount { Size = s, Count = result.Count(r => r.Size.Id == s.Id) }).ToList(),
            //    ColorCounts = Color.All.Select(c => new ColorCount { Color = c, Count = result.Count(r => r.Color.Id == c.Id) }).ToList()
            //};
        }

        private void InitializeData()
        {
            foreach (var shirt in _shirts)
            {
                var key = new KeyDefinition { Size = shirt.Size.Id, Color = shirt.Color.Id };
                if (_shirtsMap.TryGetValue(key, out List<Shirt> outList))
                {
                    outList.Add(shirt);
                }
                else
                {
                    _shirtsMap.Add(key, new List<Shirt> { shirt });
                }
            }
        }

        private struct KeyDefinition
        {
            public Guid Size { get; set; }

            public Guid Color { get; set; }
        }
    }
}