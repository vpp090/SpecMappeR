using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecMapperR
{
    public class Intro
    {
        private readonly ISpecialMapper _mapper;

        public Intro(ISpecialMapper mapper)
        {
            _mapper = mapper;

        }

        public void SampleMethod()
        {
            var sample1 = new SampleClass
            {
                Amount = 20,
                Name = "Tester"
            };
    

         var sample2 = _mapper.MapProperties<SampleClass, SampleClass2>(sample1);
        }
    }

    public class SampleClass
    {
        public int Amount;
        public string Name;
    }

    public class SampleClass2
    {
        public int Amount;
        public string Name;
    }
}
