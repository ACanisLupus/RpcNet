#!/bin/bash

./bin/Debug/RpcNetGen -n TestService -o ./Test/Generated/TestService.Generated.cs ./Test/Generated/TestService.x
./bin/Debug/RpcNetGen -n TestService -o ./Test/Generated/TestService2.Generated.cs ./Test/Generated/TestService2.x
./bin/Debug/RpcNetGen -p -n RpcNet.PortMapper -o ./RpcNet/PortMapper/PortMapper.Generated.cs ./RpcNet/PortMapper/PortMapper.x
./bin/Debug/RpcNetGen -n RpcNet.Internal -o ./RpcNet/Internal/Rpc.Generated.cs ./RpcNet/Internal/Rpc.x
