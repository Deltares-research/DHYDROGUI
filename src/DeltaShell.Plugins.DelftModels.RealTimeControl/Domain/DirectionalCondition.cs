using System;
using System.Collections.Generic;
using System.Xml.Linq;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Converters;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    [Entity]
    public class DirectionalCondition : StandardCondition
    {
        public static string TimeLagPostFix = "-1";
        
        public DirectionalCondition() : base(true)
        {
        }

        public override string GetDescription()
        {
            return new DirectionalOperationConverter().OperationToString(Operation);
        }

        public override IEnumerable<XElement> ToDataConfigExportSeries(XNamespace xNamespace, string prefix)
        {
            foreach (var export in base.ToDataConfigExportSeries(xNamespace, prefix))
            {
                yield return export;
            }
            var timeSeriesElement = new XElement(xNamespace + "timeSeries", new XAttribute("id", GetLaggedInputName()));
            yield return timeSeriesElement;
        }

        protected override XElement GetX2Element(XNamespace xNamespace, string inputName)
        {
            return new XElement(xNamespace + "x2Series",
                                Reference == string.Empty ? null : new XAttribute("ref", Reference),
                                GetLaggedInputName());
        }
        
        public override object Clone()
        {
            var directionalCondition = (DirectionalCondition)Activator.CreateInstance(GetType());
            directionalCondition.CopyFrom(this);
            return directionalCondition;
        }

        public override void CopyFrom(object source)
        {
            var directionalCondition = source as DirectionalCondition;
            if (directionalCondition != null)
            {
                base.CopyFrom(source);
            }
        }

        public string GetLaggedInputName()
        {
            return GetInputName() + TimeLagPostFix;
        }
    }
}