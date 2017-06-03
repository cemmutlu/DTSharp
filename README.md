# DTSharp

DTSharp is a .net decision tree learning library. It uses a specialized algorithm like ID3 and C4.5 to generate a decision tree. Its capabilities are listed below:

  - Pruning with depth,data count,output probability,
  - Can use any split qualifiers (like GiniImpurity,InformationGain,Entropy)
  - Can use both discrete and continious features
  - Uses Iqueryble data sources. 
  - Can use **Entity Framework** as a data source (Transfers processing load to db)
  - Easy usage with lambda expressions
 
# Usage

```sh
   var dtl = DecisionTreeLearning<PatientRecord>.Create<int>(x => x.Cancer ? 1 : 0, new DecisionTreeOptions());
    dtl.AddContiniousFeature("Age", x => x.Age);
    dtl.AddContiniousFeature("Height", x => x.Height);
    dtl.AddContiniousFeature("Weight", x => x.Weight);
    dtl.AddDiscreteFeature("Smoke", x => x.Smoke ? 1 : 0);
    var decisionTree = dtl.Learn(data);
```
   

# ToDos

  - Xml Serializable Nodes
  - Regression Tree Learning

