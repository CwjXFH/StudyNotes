import com.rabbitmq.client.*;
import lombok.SneakyThrows;

import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.HashMap;
import java.util.concurrent.ConcurrentNavigableMap;
import java.util.concurrent.ConcurrentSkipListMap;

public class RabbitMQDemo {
    private static final Print print = System.out::println;

    @SneakyThrows
    public static void main(String[] args) {
        var connFactory = new ConnectionFactory();
        //connFactory.setHost("localhost");
        //connFactory.setPort(5672);
        connFactory.setVirtualHost("/");
        connFactory.setUsername("guest");
        connFactory.setPassword("guest");
        //connFactory.setAutomaticRecoveryEnabled(true);
        //connFactory.setRequestedChannelMax(20_000);

        try (
                //var conn = connFactory.newConnection();
                // connect to multi rabbit servers(cluster)
                var conn = connFactory.newConnection(new Address[]{new Address("localhost", 5672), new Address("localhost", 5673)});
                var channel = conn.createChannel();
                //recommend: create channel for per thread
                var sendChannel = conn.createChannel();
                var receiveChannel = conn.createChannel();
        ) {
            //print.out(channel.getClass().getName());

            //var exchangeName = "j-exchange";
            //var queueName = "j-queue";
            //var exchange = channel.exchangeDeclare(exchangeName, BuiltinExchangeType.DIRECT, true);
            // void queueDeclare(String queue, boolean durable, boolean exclusive, boolean autoDelete, Map<String, Object> arguments) throws IOException
            // var queue = channel.queueDeclare(queueName, true, false, false, null);
            //channel.queueBind(queueName, exchangeName, queueName);

            var quorum_queue = "quorum_queue";
            var queueArgs = new HashMap<String, Object>();
            queueArgs.put("x-queue-type", "quorum");
            channel.queueDelete(quorum_queue);
            // create or use quorum queue must have two or more than two nodes
            // The quorum queue is a modern queue type for RabbitMQ implementing a durable,
            // replicated FIFO queue based on the Raft consensus algorithm. It is available as of RabbitMQ 3.8.0.
            // create quorum the durable property must be true and autodelete property must be false
            channel.queueDeclare(quorum_queue, true, false, false, queueArgs);

            //new Thread(() -> sendMessage(sendChannel, exchangeName, queueName)).start();
            // use default exchange and default bind
            // new Thread(() -> sendMessage(sendChannel, "", "test")).start();

            //new Thread(() -> receiveMessage(receiveChannel, "test")).start();

            //
            //var msg = receiveChannel.basicGet("test", true);
            //print.out(msg);

            //
            //var msgResp = channel.basicGet(queueName, false);
            //var msgTxt = new String(msgResp.getBody(), StandardCharsets.UTF_8);
            //print.out(msgTxt);
            //
            //channel.basicAck(msg.getEnvelope().getDeliveryTag(), false);
            //channel.basicReject();
            //channel.basicNack()

            final var read = System.in.read();
        }

    }

    private static void sendMessage(Channel channel, String exchangeName, String routingKey) {
        AMQP.Confirm.SelectOk confirm = null;
        try {
            // publisher confirm
            confirm = channel.confirmSelect();
            print.out(confirm);
        } catch (IOException e) {
            e.printStackTrace();
        }
        // async publisher confirm -- streaming confirms
        channel.addConfirmListener(new ConfirmListener() {
            @Override
            public void handleAck(long deliveryTag, boolean multiple) throws IOException {

            }

            @Override
            public void handleNack(long deliveryTag, boolean multiple) throws IOException {

            }
        });

        // if message cannot route to queue and mandatory property is true
        // the broker will return the message to the client
        channel.addReturnListener(new ReturnCallback() {
            @Override
            public void handle(Return _return) {

            }
        });

        var dateFormatter = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss.SSS");
        for (var i = 0; i < 1_500; i++) {
            var inputMsg = String.format("Send message from JAVA platform: %s.", dateFormatter.format(new Date().getTime()));
            var msgBytes = inputMsg.getBytes(StandardCharsets.UTF_8);
            print.out("send message: " + i);
            try {
                var mandatory = true;
                channel.basicPublish(exchangeName, routingKey, mandatory, null, msgBytes);
                // sync publisher confirm -- batch publishing
                //var result = channel.waitForConfirms();
                Thread.sleep(20);
            } catch (IOException e) {
                e.printStackTrace();
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
            //catch (InterruptedException e) {
            //
            //}
        }
    }

    private static void receiveMessage(Channel channel, String queueName) {
        try {
            // set prefetch count
            channel.basicQos(10, true);
            channel.basicConsume(queueName, false, new DeliverCallback() {
                @Override
                public void handle(String s, Delivery delivery) throws IOException {
                    print.out(s);
                    var msgTxt = new String(delivery.getBody(), StandardCharsets.UTF_8);
                    print.out(msgTxt);
                    var deliveryTag = delivery.getEnvelope().getDeliveryTag();
                    //channel.basicAck(deliveryTag, false);
                    channel.basicAck(deliveryTag, false);
                    //try {
                    //    Thread.sleep(20 * 1000);
                    //} catch (InterruptedException e) {
                    //    e.printStackTrace();
                    //}
                    //if (deliveryTag % 10 == 0) {
                    //    channel.basicAck(deliveryTag, true);
                    //}
                }
            }, new CancelCallback() {
                @Override
                public void handle(String s) throws IOException {
                    print.out("cancel" + s);
                }
            });
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    interface Print {
        void out(Object args);
    }
}
