namespace DelftTools.Hydro.Structures
{
    public class FixedWeir : GroupableFeature2D
    {
        

        public FixedWeir()
        {
          
        }

        public void SetupAttributeToPropertyLinks()
        {
            
        }

        public void InitializeAttributes() //should be called when events are not bubbling, but geometry is set (e.g. loading)
        {
            
        }

        public override object Clone()
        {
            var instance = (FixedWeir) base.Clone();
            return instance;
        }
    }
}