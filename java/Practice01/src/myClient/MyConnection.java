package myClient;

import java.io.Closeable;

public class MyConnection implements Closeable {
    
	public final static int RECONNECT_INTERVAL = 3000;
	
    public MyConnection(String[] uris) {
        throw new RuntimeException("Not implemented");
    }
    
    public void open() {
        throw new RuntimeException("Not implemented");
    }
    
    public void close() {
        throw new RuntimeException("Not implemented");
    }

    public int subscribe(MySubscriber subscriber) {
        throw new RuntimeException("Not implemented");
    }
    
    public boolean unsubscribe(int subscriptionId) {
    	throw new RuntimeException("Not implemented");
    }
    
    public synchronized void addConnectionListener(MyConnectionEventListener listener) {
        throw new RuntimeException("Not implemented");
    }
    
    public synchronized void removeConnectionListener(MyConnectionEventListener listener) {
        throw new RuntimeException("Not implemented");
    }
}
