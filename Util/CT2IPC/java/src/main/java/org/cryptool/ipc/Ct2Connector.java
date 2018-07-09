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
package org.cryptool.ipc;

import java.io.PrintStream;
import java.util.List;
import java.util.Map;
import java.util.concurrent.atomic.AtomicReference;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;

import org.cryptool.ipc.loops.IReceiveLoop;
import org.cryptool.ipc.loops.ISendLoop;
import org.cryptool.ipc.loops.impl.AbstractLoop.LoopState;
import org.cryptool.ipc.loops.impl.NPHelper;
import org.cryptool.ipc.loops.impl.NamedPipeReceiver;
import org.cryptool.ipc.loops.impl.NamedPipeSender;
import org.cryptool.ipc.messages.Ct2IpcMessages.Ct2IpcMessage;
import org.cryptool.ipc.messages.Ct2IpcMessages.Ct2LogEntry.LogLevel;
import org.cryptool.ipc.messages.Ct2MessageType;
import org.cryptool.ipc.messages.MessageHelper;
import org.cryptool.ipc.messages.TypedMessage;

public final class Ct2Connector {

	private static final String pipeRX = "serverToClient";
	private static final String pipeTX = "clientToServer";

	private static final Ct2Connector instance = new Ct2Connector();

	private final AtomicReference<IReceiveLoop<Ct2IpcMessage>> receiveLoop = new AtomicReference<>(null);
	private final AtomicReference<ISendLoop<TypedMessage>> sendLoop = new AtomicReference<>(null);
	private final AtomicReference<Ct2ConnectionState> connState = new AtomicReference<>(null);

	private final Lock myLock = new ReentrantLock();

	private Ct2Connector() {

	}

	public static LoopState getSenderState() {
		final ISendLoop<TypedMessage> loop = instance.sendLoop.get();
		return (loop != null) ? loop.getState() : LoopState.SHUTDOWN;
	}

	public static LoopState getReceiverState() {
		final IReceiveLoop<Ct2IpcMessage> loop = instance.receiveLoop.get();
		return (loop != null) ? loop.getState() : LoopState.SHUTDOWN;
	}

	private void shutdown_(final boolean clearState) {
		this.myLock.lock();
		try {
			final IReceiveLoop<Ct2IpcMessage> rLoop = this.receiveLoop.get();
			if (rLoop != null) {
				rLoop.stop();
			}
			final ISendLoop<TypedMessage> sLoop = this.sendLoop.get();
			if (sLoop != null) {
				sLoop.stop();
			}
			if (clearState) {
				this.connState.set(null);
			}
		} finally {
			this.myLock.unlock();
		}
	}

	private boolean start_(final TypedMessage hello, final PrintStream anErr) throws Exception {
		final PrintStream err = anErr != null ? anErr : System.err;
		this.myLock.lock();
		try {
			// shutdown and clear potential previous connection
			this.shutdown_(true);
			// create state and message loops
			final Ct2ConnectionState connState = new Ct2ConnectionState();
			final NamedPipeSender sender = new NamedPipeSender(NPHelper.pipeUrl(pipeTX + NPHelper.getPID()), err);
			final NamedPipeReceiver receiver = new NamedPipeReceiver(NPHelper.pipeUrl(pipeRX + NPHelper.getPID()),
					connState, err, sender);
			this.connState.set(connState);
			this.receiveLoop.set(receiver);
			this.sendLoop.set(sender);
			// start message loops
			receiver.start();
			sender.start();
			// send initial message
			sender.offer(hello); // TODO put initial message here
			return true;
		} finally {
			this.myLock.unlock();
		}
	}

	/**
	 * 
	 * Shuts down and clears any existing connection and tries to establish a new
	 * connection based on named pipes.
	 * 
	 * @param anErr
	 * @return
	 * @throws Exception
	 */
	public static boolean start(final String aProgramName, final String aProgramVersion, final PrintStream anErr)
			throws Exception {
		final TypedMessage hello = //
				MessageHelper.encodeCt2Hello(Ct2MessageType.myProtocolVersion, aProgramName, aProgramVersion);
		return instance.start_(hello, anErr);
	}

	public static void stop() throws Exception {
		instance.shutdown_(false);
	}

	private static boolean enqueueWithSender(final TypedMessage m) {
		final ISendLoop<TypedMessage> loop = instance.sendLoop.get();
		return (loop != null) ? loop.offer(m) : false;
	}

	public static boolean enqueueValues(final Map<Integer, String> valuesByPin) {
		if ((valuesByPin == null) || valuesByPin.isEmpty()) {
			return true;
		}
		return enqueueWithSender(MessageHelper.encodeCt2Values(valuesByPin));
	}

	public static boolean enqueueValues(final List<String> values) {
		if ((values == null) || values.isEmpty()) {
			return true;
		}
		return enqueueWithSender(MessageHelper.encodeCt2Values(values));
	}

	public static boolean encodeProgress(final double currentValue, final double maxValue) {
		return enqueueWithSender(MessageHelper.encodeCt2Progress(currentValue, maxValue));
	}

	public static boolean encodeLogEntry(final String entry, final LogLevel logLevel) {
		return enqueueWithSender(MessageHelper.encodeCt2LogEntry(entry, logLevel));
	}

	public static boolean encodeGoodbye(final int exitCode, final String exitMessage) {
		return enqueueWithSender(MessageHelper.encodeCt2GoodBye(exitCode, exitMessage));
	}

	// calls to the connection state

	public static String getServerCtName() {
		Ct2ConnectionState cs = instance.connState.get();
		return cs != null ? cs.getServerCtName() : "";
	}

	public static String getServerCtVersion() {
		Ct2ConnectionState cs = instance.connState.get();
		return cs != null ? cs.getServerCtVersion() : "";
	}

	public static boolean hasValues() {
		Ct2ConnectionState cs = instance.connState.get();
		return cs != null ? cs.hasValues() : false;
	}

	public static Map<Integer, String> getValues() {
		Ct2ConnectionState cs = instance.connState.get();
		return cs != null ? cs.getValues() : null;
	}

	public static boolean getShutdownRequested() {
		Ct2ConnectionState cs = instance.connState.get();
		return cs != null ? cs.getShutdownRequested() : false;
	}
}
