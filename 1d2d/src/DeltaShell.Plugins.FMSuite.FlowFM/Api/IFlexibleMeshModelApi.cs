using System;
using BasicModelInterface;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Api
{
    public interface IFlexibleMeshModelApi : IBasicModelInterface, IDisposable
    {
        string GetVersionString();

        bool InitializeUserTimeStep(double targetTimeRel);

        bool FinalizeUserTimeStep();

        double InitializeComputationalTimeStep(double targetTimeRel, double dt);

        double RunComputationalTimeStep(double timeStep);

        bool FinalizeComputationalTimeStep();

        Type GetVariableType(string variable);

        string GetVariableLocation(string variable);

        double GetValue(string featureCategory, string featureName, string parameterName);

        void SetValue(string featureCategory, string featureName, string parameterName, double value);

        void WriteNetGeometry(string fileName);

        void WritePartitioning(string inputFileName, string outputFileName, string polFileName);

        void WritePartitioning(string inputFileName, string outputFileName, int numDomains, bool contiguous);

        bool GetSnappedFeature(string featureType, double[] xin, double[] yin, ref double[] xout, ref double[] yout,
            ref int[] featureIds);

        void SetValuesDouble(string variable, double[] values);
        
        void SetValuesDouble(string variable, int[] start, int[] count, double[] values);
        
        void SetValuesDouble(string variable, int[] index, double[] values);
        
        void SetValuesInt(string variable, int[] values);
        
        void SetValuesInt(string variable, int[] start, int[] count, int[] values);
        
        void SetValuesInt(string variable, int[] index, int[] values);

        void Compute1d2dCoefficients();
    }
}