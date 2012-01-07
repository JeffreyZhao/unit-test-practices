package myClient;

import java.util.EventListener;
import java.util.EventObject;

public interface MyConnectionEventListener extends EventListener {
    public void connected(EventObject event);
    public void connectionFailed(EventObject event);
    public void disconnected(EventObject event);
}
