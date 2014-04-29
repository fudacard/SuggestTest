using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuggestTest
{
   public  class JSType
    {
       public string Name;
       public Dictionary<string, JSType> Members;
       public JSType Super;

       public JSType()
       {
           Members = new Dictionary<string, JSType>();
       }

       public JSType(string name)
       {
           this.Name = name;
           Members = new Dictionary<string, JSType>();
       }
       public Dictionary<string, JSType> GetMembers()
       {
           Dictionary<string, JSType> result = new Dictionary<string,JSType>(Members);
           if (Super != null) {
               Dictionary<string, JSType> superList = Super.GetMembers();
               foreach(var pair in superList) {
                   if (!result.ContainsKey(pair.Key)) {
                       result.Add(pair.Key, pair.Value);
                   }
               }
           }
           return result;
       }
    }
}
