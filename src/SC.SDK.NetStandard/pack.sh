#!/bin/bash
dotnet pack
cp -rf bin/debug/*.nupkg ../../packages
