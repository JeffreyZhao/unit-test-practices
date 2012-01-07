package myDriver;

public final class MyData {
    
    public final int queryId;
    public final String value;
    
    public MyData(int queryId, String value) {
        this.queryId = queryId;
        this.value = value;
    }
    
    public String toString() {
        return this.queryId + ", " + this.value;
    }
}
