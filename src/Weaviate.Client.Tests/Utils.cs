using System.Diagnostics;

namespace Weaviate.Client.Tests
{
    public class TestTraceListener : TraceListener
    {
        private readonly ITestOutputHelper output;
        StringWriter _writer = new StringWriter();

        public TestTraceListener(ITestOutputHelper output)
        {
            this.output = output;

            Trace.Listeners.Add(this);
        }

        // called (in debug-mode) when Debug.Write() is called
        public override void Write(string? message)
        {
            ArgumentNullException.ThrowIfNull(message);

            _writer.Write(message);
        }
        // called (in debug-mode) when Debug.WriteLine() is called
        public override void WriteLine(string? message)
        {
            ArgumentNullException.ThrowIfNull(message);

            _writer.WriteLine(message);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Trace.Listeners.Remove(this);
                output.WriteLine(_writer.ToString());
                _writer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}