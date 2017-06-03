using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using DTSharp.DecisionTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DTSharp.Tests
{
    [TestClass]
    public class DecisionTreeTests
    {
        [TestMethod]
        public void PlayGolfExample()
        {
            //http://www.saedsayad.com/decision_tree.htm
            var outlook = new int[] { 0, 0, 1, 2, 2, 2, 1, 0, 0, 2, 0, 1, 1, 2 };
            var temp = new int[] { 0, 0, 0, 1, 2, 2, 2, 1, 2, 1, 1, 1, 0, 1 };
            var humidity = new int[] { 0, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 0 };
            var wind = new int[] { 0, 1, 0, 0, 0, 1, 1, 0, 0, 0, 1, 1, 0, 1 };
            var output = new int[] { 0, 0, 1, 1, 1, 0, 1, 0, 1, 1, 1, 1, 1, 0 };
            DecisionTreeLearning<int> dtl = DecisionTreeLearning<int>.Create<int>(x => output[x], new DecisionTreeOptions()
            {
                MaxDepth = 3,
            });
            var outlookStrs = new string[] { "Rainy", "Overcast", "Sunny" };
            dtl.AddDiscreteFeature<string>("outlook", a => outlookStrs[outlook[a]]);
            var tempStrs = new string[] { "Hot", "Mild", "Cool" };
            dtl.AddDiscreteFeature<string>("temp", a => tempStrs[temp[a]]);
            dtl.AddDiscreteFeature<int>("humidity", a => humidity[a]);
            dtl.AddDiscreteFeature<int>("wind", a => wind[a]);
            var dt = dtl.Learn(Enumerable.Range(0, output.Length));

            Assert.AreEqual(dt.Feature.Name, "outlook");
            Assert.AreEqual(dt.ChildNodes.First(x => (x.Key as DiscreteFeatureValue).Value.ToString() == "Rainy").Value.Feature.Name, "humidity");
            Assert.AreEqual(dt.ChildNodes.First(x => (x.Key as DiscreteFeatureValue).Value.ToString() == "Sunny").Value.Feature.Name, "wind");
            Assert.IsNull(dt.ChildNodes.First(x => (x.Key as DiscreteFeatureValue).Value.ToString() == "Overcast").Value.Feature, "Overcast node should be leaf");

            Assert.AreEqual(dt.GetOutput(0), 0);
            Assert.AreEqual(dt.GetOutput(2), 1);
        }
        [TestMethod]
        public void ContiniousFeatureTest()
        {
            var data = Enumerable.Range(0, 1000).Select(x => new PatientRecord()).ToArray();

            var dt = DecisionTreeLearning<PatientRecord>.Create<int>(x => x.Cancer ? 1 : 0, new DecisionTreeOptions());
            dt.AddContiniousFeature("Age", x => x.Age);
            dt.AddContiniousFeature("Height", x => x.Height);
            dt.AddContiniousFeature("Weight", x => x.Weight);
            dt.AddDiscreteFeature("Smoke", x => x.Smoke ? 1 : 0);
            var node = dt.Learn(data);
            Assert.AreEqual(node.Feature.Name, "Smoke");
        }


        class PatientRecord
        {
            static Random random = new Random();
            public int Age { get; set; }
            public int Height { get; set; }
            public int Weight { get; set; }
            public bool Smoke { get; set; }
            public bool Cancer { get; set; }
            public PatientRecord()
            {
                Age = random.Next(18, 80);
                Height = random.Next(150, 200);
                Weight = random.Next(50, 150);
                Smoke = random.NextDouble() < 0.3;

                double cancerchance = 0.05;
                if (Age > 30 && Age < 50) cancerchance *= 2;
                if (Weight > 100) cancerchance *= 2;
                if (Height > 175) cancerchance *= 2;
                if (Smoke) cancerchance *= 8;
                else cancerchance /= 2;
                Cancer = random.NextDouble() < cancerchance;
            }
        }
    }

}
