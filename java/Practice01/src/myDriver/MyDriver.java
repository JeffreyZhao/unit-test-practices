package myDriver;

import java.util.HashSet;
import java.util.Set;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.atomic.AtomicBoolean;

public final class MyDriver {
	
	private Set<Integer> _queryIds = new HashSet<Integer>();
	private BlockingQueue<MyData> _dataQueue = new LinkedBlockingQueue<MyData>();
	private AtomicBoolean _isClosed = new AtomicBoolean(false);
	
	public MyDriver(String uri) {
		new Thread(new Runnable() {
			
			@Override
			public void run() {
				MyDriver.this.feedData();
			}
			
		}).start();
	}
	
	public void addQuery(int id) {
		if (Math.random() < 0.05) {
            throw new MyDriverException("Error occurred when add query.");
        }
		
		synchronized (this._queryIds) {
			this._queryIds.add(id);
			this._dataQueue.add(new MyData(id, "begin"));
		}
	}
	
	public void removeQuery(int id) {
		if (Math.random() < 0.05) {
            throw new MyDriverException("Error occurred when receive query.");
        }
		
		synchronized (this._queryIds) {
			this._queryIds.remove(id);
		}
	}
	
	public void close() {
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
            throw new MyDriverException("Error occurred when receive.");
        }

        try {
			return this._dataQueue.take();
		} catch (InterruptedException e) {
			return null;
		}
	}
}
