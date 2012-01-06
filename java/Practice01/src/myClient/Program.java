package myClient;

public class Program {

	/**
	 * @param args
	 */
	public static void main(String[] args) {
		
		final MySender sender = new MySender("jeffz://server:12345");

        try {
            sender.addQuery(1);
            sender.addQuery(2);
            sender.addQuery(3);
        }
        catch (MyClientException ex) {
            System.out.println("Error occurred when add query.");
            System.exit(1);
        }

        new Thread(new Runnable() {

			@Override
			public void run() {
				receiveData(sender.getReceiver());
			}
			
        }).start();
	}

	private static void receiveData(MyReceiver receiver) {
		try {
            while (true) {
                MyData data = receiver.receive();
                if (data == null) {
                    System.out.println("Closed");
                    break;
                }
                else {
                    System.out.println(data);
                }
            }
        }
        catch (MyClientException ex) {
            System.out.println("Error occurred when receive data.");
        }
	}
}
