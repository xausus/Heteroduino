using System;
using System.Linq;
using Grasshopper.Kernel;

namespace Heteroduino
{
   

  

    class Tools
    {


        public static bool AddCoreRX(GH_Component comp)
        {
            if (comp.Params.Input[0].SourceCount > 0) return true;
            var os =comp. OnPingDocument().Objects.Where(i => i is Core).ToList();
            if (os.Count == 0) return false;
            var levelDif = os.Select(i => 
            Math.Abs(i.Attributes.Pivot.Y - comp.Attributes.Pivot.Y)).ToList();
            var index = levelDif.IndexOf(levelDif.Min());
            var _o = os[index];

            if (_o ==null) return false;
            var _core = _o as Core;
         //   if (_core == null) return false;
            var rxn = _core.Params.Output[0];
            comp.Params.Input[0].AddSource(rxn);
            return true;

        }
        public static void GetSource(GH_Document doc, IGH_Param Reciever, int index)
        {
            var os = doc.Objects.Where(i => i is Core).Cast<Core>().ToList();
            if (os.Count == 0) return ;
            var levelDif = os.Select(i =>
            Math.Abs(i.Params.Output[0].Attributes.Pivot.Y - Reciever.Attributes.Pivot.Y)).ToList();
            var dex = levelDif.IndexOf(levelDif.Min());
            var r = os[dex].Params.Output[index];
              Reciever.RemoveAllSources(); Reciever.AddSource(r);
        }

        public static bool checkmegatx(GH_Component comp)
        {
            var rc = comp.Params.Output[0].Recipients ;
            return rc.Count > 0 && ((TX)rc[0].Attributes.Parent.DocObject)?.Megaset == true;
        }
     



        public static void GetSource<T>(GH_Document doc, IGH_Param Reciever, int index) where T : GH_Component   //, new()
        {
            // var id = new T().ComponentGuid;
          
            foreach (var r in doc.Objects.Where(i => i is T).Cast<T>()
                .Select(i => i.Params.Output[index])
                .Where(i =>! Reciever.Sources.Contains(Reciever))) Reciever.AddSource(r);
        }
      
        public static T FindComp<T>(GH_Document doc) where T : GH_Component  , new()
        => (T) doc.Objects.FirstOrDefault(i => i is T);

        public static TX FindTX(GH_Document doc) 
        => (TX)doc.Objects.FirstOrDefault(i => i is TX);

        public static bool ToArduino(string command,GH_Document doc)
        {
            var tx = FindTX(doc);
           return tx?.ForceSerialsend(command) ?? false;
         
        }


       
        public static bool Connectparam<T>(GH_Document doc, IGH_Param source, int index) where T : GH_Component, new()
        {

            foreach (IGH_Param t in source.Recipients.ToList())
                t.RemoveSource(source.InstanceGuid);
            try
            {
 var ps = doc.Objects.Where(i => i.Attributes.IsTopLevel && i is T) .Cast<T>()
                    .Select(i=>i  .Params.Input[index]).ToList();

                var levelDif = ps.Select(i =>
          Math.Abs(i.Attributes.Pivot.Y - source.Attributes.Pivot.Y)).ToList();
                var dex = levelDif.IndexOf(levelDif.Min());
                 ps[dex].AddSource(source);


                if (ps.Count == 0) return false;

          
            }
            catch
            {
                return false;
            }
 return true;
        }
    } 
}
