package myClient;

public class MySender {
	private final MyReceiver _receiver = new MyReceiver();
	
	public MySender(String uri) { }
	
	public MyReceiver getReceiver() {
		return this._receiver;
	}
	
	public void addQuery(int queryId) {
		if (Math.random() < 0.05) {
            throw new MyClientException("Error occurred when add query.");
        }
		
		this._receiver.addQuery(queryId);
	}
	
	public void removeQuery(int queryId) {
		if (Math.random() < 0.05) {
            throw new MyClientException("Error occurred when remove query.");
        }
		
		this._receiver.removeQuery(queryId);
	}
	
	public void close() {
		this._receiver.close();
	}
}
