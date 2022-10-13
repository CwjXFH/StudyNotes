/*
 * Build: base on OpenJDK17
 * Maven packages:
 * com.rabbitmq.amqp-client
 * ch.qos.logback.logback-classic
 * */

import com.rabbitmq.client.*;
import lombok.SneakyThrows;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.HashMap;
import java.util.Map;
import java.util.concurrent.ConcurrentNavigableMap;
import java.util.concurrent.ConcurrentSkipListMap;
import java.util.stream.Collectors;

public class RabbitMQDemo {
    private static final Print print = System.out::println;
    private static final Logger logger = LoggerFactory.getLogger(RabbitMQDemo.class);

    @SneakyThrows
    public static void main(String[] args) {
        logger.info("begin...");
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
                var conn = connFactory.newConnection(new Address[]{
                        new Address("localhost", 5674)
                        , new Address("localhost", 5673)
                });
                var channel = conn.createChannel();
                //recommend: create channel for per thread
                var sendChannel = conn.createChannel();
                var receiveChannel = conn.createChannel();
                var receiveChannel2 = conn.createChannel();
        ) {
            //print.out(channel.getClass().getName());

            var exchangeName = "j-exchange";
            var queueName = "j-queue";
            var exchange = channel.exchangeDeclare(exchangeName, BuiltinExchangeType.DIRECT, true);
            // //void queueDeclare(String queue, boolean durable, boolean exclusive, boolean autoDelete, Map<String, Object> arguments) throws IOException
            var queue = channel.queueDeclare(queueName, true, false, false, null);
            channel.queueBind(queueName, exchangeName, queueName);
            //new Thread(() -> sendMessage(sendChannel, "", queueName)).start();
            // The queue can has multi consumer
            // different consumer on the same channel has different behavior of different consumer on different channel
            // prefetch count property and ack
            new Thread(() -> receiveMessage(receiveChannel, queueName)).start();
            new Thread(() -> receiveMessage(receiveChannel, queueName)).start();
            //new Thread(() -> receiveMessage(receiveChannel2, queueName)).start();


            //var quorum_queue = "quorum_queue";
            //var queueArgs = new HashMap<String, Object>();
            //queueArgs.put("x-queue-type", "quorum");
            //// The group size greater than zero and smaller or equal to the current RabbitMQ cluster size. Default replicas is 3.
            //// The quorum queue will be launched to run on a random subset of RabbitMQ nodes present in the cluster at declaration time.
            // queueArgs.put("x-quorum-initial-group-size", 9);
            ////channel.queueDelete(quorum_queue);

            // create or use quorum queue must have two or more than two nodes
            // The quorum queue is a modern queue type for RabbitMQ implementing a durable,
            // replicated FIFO queue based on the Raft consensus algorithm. It is available as of RabbitMQ 3.8.0.
            // create quorum the durable property must be true and autodelete property must be false
            //channel.queueDeclare(quorum_queue, true, false, false, queueArgs);

            // use default exchange and default bind
            //new Thread(() -> sendMessage(sendChannel, "", quorum_queue)).start();
            //new Thread(() -> receiveMessage(receiveChannel, quorum_queue)).start();
            //new Thread(() -> receiveMessage(receiveChannel, quorum_queue, true)).start();


            //var stream_queue = "stream_queue";
            //var queueArgs = new HashMap<String, Object>();
            //queueArgs.put("x-queue-type", "stream");
            //channel.queueDeclare(stream_queue, true, false, false, queueArgs);
            ////new Thread(() -> sendMessage(sendChannel, "", stream_queue));
            //new Thread(() -> receiveMessage(receiveChannel, stream_queue));


            //new Thread(() -> sendMessage(sendChannel, exchangeName, queueName)).start();
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
        //try {
        //    // publisher confirm
        //    //confirm = channel.confirmSelect();
        //    print.out(confirm);
        //} catch (IOException e) {
        //    e.printStackTrace();
        //}
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
        for (var i = 0; i < 100_500; i++) {
            var inputMsg = String.format("Send message from JAVA platform: %s.", dateFormatter.format(System.currentTimeMillis()));
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
        receiveMessage(channel, queueName, false);
    }

    private static void receiveMessage(Channel channel, String queueName, boolean autoAck) {
        try {
            // set prefetch count, for quorum queue prefetch must be global mode
            channel.basicQos(10, true);
            //basicConsume(String queue, boolean autoAck, DeliverCallback deliverCallback, CancelCallback cancelCallback)
            //automatic message acknowledgement should be considered unsafe and not suitable for all workloads.
            //reference: https://www.rabbitmq.com/confirms.html#acknowledgement-modes
            channel.basicConsume(queueName, autoAck, new DeliverCallback() {
                        @Override
                        public void handle(String consumerTag, Delivery delivery) throws IOException {
                            var msgTxt = new String(delivery.getBody(), StandardCharsets.UTF_8);
                            print.out(consumerTag + " receive message: " + msgTxt);
                            var deliveryTag = delivery.getEnvelope().getDeliveryTag();
                            //basicAck(long deliveryTag, boolean multiple)
                            if (autoAck == false) {
                                //channel.basicAck(deliveryTag, false);
                            }
                            //basicNack(long deliveryTag, boolean multiple, boolean requeue)
                            //basicReject(long deliveryTag, boolean requeue)
                            //channel.basicReject(deliveryTag, false);

                            //channel.basicCancel(consumerTag);
                            // will call CancelCallback.handle method
                            //channel.queueDelete(queueName);

                            //try {
                            //    Thread.sleep(20 * 1000);
                            //} catch (InterruptedException e) {
                            //    e.printStackTrace();
                            //}
                            //if (deliveryTag % 10 == 0) {
                            //    channel.basicAck(deliveryTag, true);
                            //}
                        }
                    }
                    // consumer cancel notification
                    // when consumer cancel subscription the queue(except call channel.basicCancel), will call this method
                    , new CancelCallback() {
                        @Override
                        public void handle(String consumerTag) throws IOException {
                            print.out(consumerTag + "has been cancelled");
                        }
                    });
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    private static void receiveStream(Channel channel, String queueName, Offset offset) {
        Map<String, String> args = new HashMap<String, String>();
        args.put("x-stream-offset", offset.value);
        //try {
        //    // public String basicConsume(String queue, boolean autoAck, Map<String, Object> arguments,
        //    // DeliverCallback deliverCallback, CancelCallback cancelCallback) throws IOException {
        //    //Collectors.singletonMap("","");
        //    channel.basicConsume(queueName, false, args, new DeliverCallback() {
        //        @Override
        //        public void handle(String s, Delivery delivery) throws IOException {
        //
        //        }
        //    }, new CancelCallback() {
        //        @Override
        //        public void handle(String s) throws IOException {
        //
        //        }
        //    });
        //} catch (IOException e) {
        //
        //}
    }

    interface Print {
        void out(Object args);
    }

    enum Offset {
        First("first"),
        Last("last");

        private String value;

        private Offset(String value) {
            this.value = value;
        }

        @Override
        public String toString() {
            return this.value;
        }
    }
}
