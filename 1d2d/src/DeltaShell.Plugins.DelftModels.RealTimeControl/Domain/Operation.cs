namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    // based on rtcToolsConfig.xsd
    //<simpleType name="relationalOperatorEnumStringType">
    //  <annotation>
    //    <documentation>relational operator</documentation>
    //  </annotation>
    //  <restriction base="string">
    //    <enumeration value="Less"/>
    //    <enumeration value="LessEqual"/>
    //    <enumeration value="Equal"/>
    //    <enumeration value="Unequal"/>
    //    <enumeration value="GreaterEqual"/>
    //    <enumeration value="Greater"/>
    //  </restriction>
    //</simpleType>
    public enum Operation
    {
        Equal,
        Unequal,
        Less,
        LessEqual,
        Greater,
        GreaterEqual
    }
}