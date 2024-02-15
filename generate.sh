#!/bin/bash

./bin/Debug/RpcNetGen -u -n TestService -o ./Test/Generated/TestService.Generated.cs ./Test/Generated/TestService.x
./bin/Debug/RpcNetGen -u -n TestService -o ./Test/Generated/TestService2.Generated.cs ./Test/Generated/TestService2.x
./bin/Debug/RpcNetGen -p -u -n RpcNet.PortMapper -o ./RpcNet/PortMapper/PortMapper.Generated.cs ./RpcNet/PortMapper/PortMapper.x
./bin/Debug/RpcNetGen -u -n RpcNet.Internal -o ./RpcNet/Internal/Rpc.Generated.cs ./RpcNet/Internal/Rpc.x
