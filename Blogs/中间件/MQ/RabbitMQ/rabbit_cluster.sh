# rabbitmq cluster:
# five nodes: node1..node5
# node1 & node2 is disk node, others are ram node

gen_container_name(){
    container_name=rabbitmq$1
}

init(){
	for i in {1..5}; do
        gen_container_name $i

		echo "<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<stop container $container_name"
		docker stop $container_name
		echo " >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> delete container $container_name"
		docker rm -v $container_name
	done

	echo " >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> remove volume rabbit_erl"
	docker volume rm rabbit_erl
	echo " >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> remove network rabbit_cluster"
	docker network rm rabbit_cluster

	docker volume create rabbit_erl
	docker network create rabbit_cluster
}

create_container(){
    for i in {1..5}; do
        gen_container_name $i
    
    	echo "=========================================create container $container_name"
    
    	docker run -d --name $container_name --hostname node$i --network rabbit_cluster -v rabbit_erl:/var/lib/rabbitmq -p 567$i:5672 -p 1567$i:15672 -e RABBITMQ_NODENAME=r$i     rabbitmq:3-management
    done
}

join_cluster(){
    for i in {2..5}; do
        gen_container_name $i

    	echo "=========================================$container_name join cluster node type $node_type"
    	docker exec $container_name rabbitmqctl stop_app
    	docker exec $container_name rabbitmqctl reset
    	if (($i == 2)); then
    		docker exec $container_name rabbitmqctl join_cluster r1@node1
    	else
    		docker exec $container_name rabbitmqctl join_cluster --ram r1@node1
    	fi
    	docker exec $container_name rabbitmqctl start_app
    done
}

init
create_container

# Pause for a few seconds to make sure the container finishes starting
echo "=========================================sleep 3 seconds"
sleep 3

join_cluster

echo ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>remove variables"
unset container_name
unset init
unset create_container
unset join_cluster
