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

import java.io.PrintStream;
import java.util.concurrent.atomic.AtomicReference;

import org.cryptool.ipc.loops.ILoop;

public abstract class AbstractLoop<E> implements Runnable, ILoop {

	// children need to implement run()

	static final int MaxConnectionErrors = 50;
	static final long DelayOnConnectionError = 200;

	// static final int MaxMessageErrors = 5;
	// static final long DelayOnMessageError = 100;

	// detect queue as empty and shutdown after at most 250 milliseconds
	static final int LoopstateUpdatePeriod = 10;
	static final int DrainPeriod = 5;
	static final long MaxLoopSleep = 25;
	static final long LoopSleepIncrement = 5;

	public enum LoopState {
		SHUTDOWN, SHUTTINGDOWN, RUNNING, STARTING;
	}

	final AtomicReference<LoopState> myState = new AtomicReference<LoopState>(LoopState.SHUTDOWN);

	private final PrintStream err;

	AbstractLoop(final PrintStream anErr) {
		this.err = (anErr != null) ? anErr : System.err;
	}

	@Override
	public boolean isRunning() {
		return this.myState.get() == LoopState.RUNNING;
	}

	protected void printErr(final String message) {
		this.printErr(message, null);
	}

	protected void printErr(final String message, final Throwable e) {
		this.err.println(this.getClass().getSimpleName() + ": " + message);
		if (e != null) {
			this.err.println(e.getMessage());
		}
	}

	@Override
	public boolean start() {
		if (this.myState.compareAndSet(LoopState.SHUTDOWN, LoopState.STARTING)) {
			new Thread(this).start();
			return true;
		}
		return false;
	}

	@Override
	public boolean stop() {
		if (!this.myState.compareAndSet(LoopState.STARTING, LoopState.SHUTTINGDOWN)) {
			return this.myState.compareAndSet(LoopState.RUNNING, LoopState.SHUTTINGDOWN);
		}
		return true;
	}

	@Override
	public LoopState getState() {
		return this.myState.get();
	}

}
