/*
   Copyright 2018 Henner Heck

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
package org.cryptool.ipc.loops.impl;

import java.io.IOException;
import java.io.OutputStream;
import java.io.PrintStream;
import java.io.RandomAccessFile;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.TimeUnit;

import org.cryptool.ipc.loops.ISendLoop;
import org.cryptool.ipc.messages.Ct2IpcMessages.Ct2IpcMessage;
import org.cryptool.ipc.messages.TypedMessage;

public class NamedPipeSender extends AbstractLoop<TypedMessage> implements ISendLoop<TypedMessage> {

	private final BlockingQueue<TypedMessage> queue = new LinkedBlockingQueue<>();
	private final String pipeUrl;

	public NamedPipeSender(final String aPipeUrl, final PrintStream anErr) {
		super(anErr);
		this.pipeUrl = aPipeUrl;
	}

	@Override
	public boolean offer(TypedMessage message) {
		return this.queue.offer(message);
	}

	@Override
	public void run() {
		if (this.myState.compareAndSet(LoopState.STARTING, LoopState.RUNNING)) {
			long sequenceNumber = 0L;
			try {
				final RandomAccessFile pipe = NPHelper.connectPipe(this.pipeUrl, "rws",
						AbstractLoop.DelayOnConnectionError, AbstractLoop.MaxConnectionErrors, this.myState);
				if (this.myState.get() != LoopState.RUNNING) {
					return;
				}
				if (pipe == null) {
					this.printErr("Could not connect to named pipe \"" + this.pipeUrl
							+ "\". Message sender is shutting down.");
					this.setStopped();
					return;
				}
				OutputStream os = NPHelper.getOutputStream(pipe, DelayOnConnectionError, MaxConnectionErrors);
				if (os == null) {
					this.printErr("Could not create an output stream from pipe \"" + this.pipeUrl
							+ "\". Message sender is shutting down.");
					this.setStopped();
					return;
				}
				// possibly unnecessary optimization to avoid
				// polling the atomic boolean on each loop
				int stateUpdateCounter = 0;
				LoopState state = this.myState.get();
				try {
					while (state == LoopState.RUNNING) {
						final TypedMessage m = this.queue.poll(AbstractLoop.MaxLoopSleep, TimeUnit.MILLISECONDS);
						if (m != null) {
							Ct2IpcMessage.newBuilder()//
									.setSequenceNumber(++sequenceNumber)//
									.setMessageType(m.getType().getId())//
									.setBody(m.getEncodedMessage())//
									.build()//
									.writeDelimitedTo(os);
						}
						if (++stateUpdateCounter >= AbstractLoop.LoopstateUpdatePeriod) {
							state = this.myState.get();
							stateUpdateCounter = 0;
						}
					}
				} catch (IOException e) {
					this.printErr("Message sender encountered an I/O error and is shutting down.");
				}
			} catch (InterruptedException e) {
				this.printErr("Message sender was interrupted and is shutting down.");
			}
			// The message receiver shuts down.
			// The pipe must be closed by cryptool.
			this.setStopped();
		}
	}

	private void setStopped() {
		this.myState.set(LoopState.SHUTDOWN);
	}

}
