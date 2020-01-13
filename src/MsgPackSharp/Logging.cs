namespace MsgPackSharp
{
    public static class Logging
    {
        public static ILogFactory Factory { get; set; }

        public static ILog GetLog(string name) => (Factory ?? NoLogFactory.Instance).Get(name);

        public static ILog GetLog<T>() => GetLog(typeof(T).FullName);
    }

    public interface ILogFactory
    {
        ILog Get(string name);
    }

    public interface ILog
    {
        void Trc(string fmt, params object[] args);
        void Dbg(string fmt, params object[] args);
        void Inf(string fmt, params object[] args);
        void Wrn(string fmt, params object[] args);
        void Err(string fmt, params object[] args);
        void Ftl(string fmt, params object[] args);
    }

    internal class NoLogFactory : ILogFactory
    {
        public static readonly ILogFactory Instance = new NoLogFactory();

        public ILog Get(string name) => NoLog.Instance;
    }

    internal class NoLog : ILog
    {
        public static readonly ILog Instance = new NoLog();

        public void Trc(string fmt, params object[] args) {}
        public void Dbg(string fmt, params object[] args) {}
        public void Inf(string fmt, params object[] args) {}
        public void Wrn(string fmt, params object[] args) {}
        public void Err(string fmt, params object[] args) {}
        public void Ftl(string fmt, params object[] args) {}
    }
}