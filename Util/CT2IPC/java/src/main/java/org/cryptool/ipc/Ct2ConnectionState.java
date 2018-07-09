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

import java.util.Map;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.atomic.AtomicBoolean;
import java.util.concurrent.atomic.AtomicInteger;
import java.util.concurrent.atomic.AtomicReference;

import org.cryptool.ipc.messages.Ct2MessageType;

public final class Ct2ConnectionState {

	private static final int unknownInt = -1;

	private final AtomicInteger serverProtocolVersion = new AtomicInteger(unknownInt);

	private final AtomicReference<String> serverCtName = new AtomicReference<String>("");
	private final AtomicReference<String> serverCtVersion = new AtomicReference<String>("");

	private final BlockingQueue<Map<Integer, String>> valuesByIndex = new LinkedBlockingQueue<Map<Integer, String>>();

	private final AtomicBoolean shutdownRequested = new AtomicBoolean(false);

	public static boolean supportedProtocol(final int aProtocolVersion) {
		return Ct2MessageType.myProtocolVersion >= aProtocolVersion;
	}

	int getServerProtocolVersion() {
		return this.serverProtocolVersion.get();
	}

	public void setServerProtocolVersion(final int version) {
		this.serverProtocolVersion.set(version);
	}

	public void setServerCtName(final String name) {
		this.serverCtName.set(name);
	}

	public void setServerCtVersion(final String version) {
		this.serverCtVersion.set(version);
	}

	public boolean addValues(final Map<Integer, String> values) {
		return this.valuesByIndex.offer(values);
	}

	public boolean setShutdownRequested() {
		return this.shutdownRequested.getAndSet(true);
	}

	String getServerCtName() {
		return this.serverCtName.get();
	}

	String getServerCtVersion() {
		return this.serverCtVersion.get();
	}

	boolean hasValues() {
		return !this.valuesByIndex.isEmpty();
	}

	Map<Integer, String> getValues() {
		return this.valuesByIndex.poll();
	}

	boolean getShutdownRequested() {
		return this.shutdownRequested.get();
	}

}
