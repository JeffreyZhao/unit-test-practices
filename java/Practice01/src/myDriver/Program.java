package myDriver;

public class Program {

	/**
	 * @param args
	 */
	public static void main(String[] args) {
		
		final MyDriver driver = new MyDriver("jeffz://server:12345");

        try {
            driver.addQuery(1);
            driver.addQuery(2);
            driver.addQuery(3);
        }
        catch (MyDriverException ex) {
            System.out.println("Error occurred when add query.");
            System.exit(1);
        }

        new Thread(new Runnable() {

			@Override
			public void run() {
				receiveData(driver);
			}
			
        }).start();
	}

	private static void receiveData(MyDriver driver) {
		try {
            while (true) {
                MyData data = driver.receive();
                if (data == null) {
                    System.out.println("Closed");
                    break;
                }
                else {
                    System.out.println(data);
                }
            }
        }
        catch (MyDriverException ex) {
            System.out.println("Error occurred when receive data.");
        }
	}
}
