using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using voluntLib.common.eventArgs;
using voluntLib.common.interfaces;

namespace voluntLib.common.interfaces{

public abstract class AWorker
{
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public byte[] JobID { get; set; }
        public abstract CalculationResult DoWork(byte[] jobPayload, BigInteger blockID, CancellationToken cancelToken);

        public event EventHandler<TaskEventArgs> ProgressChanged;

        protected virtual void OnProgressChanged(BigInteger blockID, int progress)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var handler = ProgressChanged;
                    if (handler != null)
                    {
                        handler(this, new TaskEventArgs(JobID, blockID, TaskEventArgType.Progress) { TaskProgress = progress });
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("Error in OnProgressChanged" + e.GetType() + "(see debug log for stacktrace");
                    Logger.Debug("Error in OnProgressChanged: " + e.StackTrace);
                }
            });
        }
        }
}