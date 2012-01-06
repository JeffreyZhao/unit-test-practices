package myClient;

import java.util.HashSet;
import java.util.Set;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.atomic.AtomicBoolean;

public final class MyReceiver {
	
	private Set<Integer> _queryIds = new HashSet<Integer>();
	private BlockingQueue<MyData> _dataQueue = new LinkedBlockingQueue<MyData>();
	private AtomicBoolean _isClosed = new AtomicBoolean(false);
	
	protected MyReceiver() {
		new Thread(new Runnable() {
			
			@Override
			public void run() {
				MyReceiver.this.feedData();
			}
			
		}).start();
	}
	
	protected void addQuery(int id) {
		synchronized (this._queryIds) {
			this._queryIds.add(id);
			this._dataQueue.add(new MyData(id, "begin"));
		}
	}
	
	protected void removeQuery(int id) {
		synchronized (this._queryIds) {
			this._queryIds.remove(id);
		}
	}
	
	protected void close() {
		synchronized (this._queryIds) {
			this._isClosed.set(true);
			this._dataQueue.clear();
			this._dataQueue.add(null);
		}
	}
	
	private void feedData() {
		
		while (!this._isClosed.get()) {
			
			try {
				Thread.sleep(500);
			} catch (InterruptedException e) { }

            synchronized (this._queryIds) {
                for (int id : this._queryIds) {
                    this._dataQueue.add(new MyData(id, Double.toString(Math.random())));
                }
            }
        }
	}
	
	public MyData receive() {
		
		if (this._isClosed.get()) return null;

        if (Math.random() < 0.01) {
            throw new MyClientException("Error occurred when receive.");
        }

        try {
			return this._dataQueue.take();
		} catch (InterruptedException e) {
			return null;
		}
	}
}
