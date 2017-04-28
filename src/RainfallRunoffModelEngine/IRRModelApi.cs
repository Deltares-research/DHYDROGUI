namespace RainfallRunoffModelEngine
{
    public interface IRRModelApi
    {
        #region Model

        bool ModelInitialize();
        int ModelGetCurrentTime();
        bool ModelPerformTimeStep();
        bool ModelFinalize();

        #endregion

        #region Runtime

        void GetValues(QuantityType quantity, ElementSet elmSet, ref double[] values, int location);
        void GetValues(QuantityType quantity, ElementSet elmSet, ref double[] values);
        double GetValue(QuantityType quantity, ElementSet elmSet, int location);
        int GetSize(ElementSet elmSet);
        bool SetValues(QuantityType quantity, ElementSet elmSet, double[] values);
        bool SetValue(QuantityType quantity, ElementSet elmSet, double value, int location);

        #endregion

        #region Logging

        string GetError(int errorId);
        void LogMessages();
        int MessageCount();
        string GetMessage(int messageId, out ErrorLevel errorLevel);

        #endregion
    }
}