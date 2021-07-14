/********************
Copyright Notice
Copyright © 2019, The Regents of the University of California
All rights reserved.

Please see the file LICENSE in this distribution for license terms.
**********************/
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace EmbeddedAdaptiveDBSApplication.ViewModels
{
    public partial class MainViewModel : Screen
    {
        private static Thread ZeroMQThread;

        public void StartStreamLogButton()
        {
            ZeroMQThread = new Thread(new ThreadStart(ZeroMQThreadCode));
            ZeroMQThread.IsBackground = true;
            ZeroMQThread.Start();
        }

        public void StopStreamLogButton()
        {
            ZeroMQThread.Abort();
            ZeroMQThread = null;
        }

        private void ZeroMQThreadCode()
        {
            using (var responder = new ZSocket(ZSocketType.REP))
            {
                // Bind
                responder.Bind("tcp://*:5555");

                while (true)
                {
                    // Receive
                    using (ZFrame request = responder.ReceiveFrame())
                    {
                        string response = request.ReadString();
                        Messages.Insert(0, DateTime.Now + ":: Received: " + response);

                        try
                        {
                            bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, "Stream Log Data", response);
                            CheckForReturnErrorInLog(bufferReturnInfo, "Logging Stream Log Data");
                            responder.Send(new ZFrame("Successful: " + bufferReturnInfo.Descriptor));
                        }
                        catch(Exception e)
                        {
                            _log.Error(e);
                            responder.Send(new ZFrame("Unsuccessful: " + bufferReturnInfo.Descriptor + ". Reject code: " + bufferReturnInfo.RejectCode));
                        }
                    }
                }
            }
        }
    }
}
