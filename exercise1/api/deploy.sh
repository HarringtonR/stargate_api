#!/bin/bash

# Build and deploy script for AWS Elastic Beanstalk
echo "Starting deployment process for .NET Core on Linux..."

# Clean previous builds
echo "Cleaning previous builds..."
dotnet clean
rm -rf bin/
rm -rf obj/
rm -rf publish/
rm -f stargate-api-deployment.zip

# Restore dependencies
echo "Restoring dependencies..."
dotnet restore

# Build the application
echo "Building application..."
dotnet build -c Release

# Publish the application with proper runtime configuration
echo "Publishing application for Linux with runtime configuration..."
dotnet publish -c Release -o ./publish --runtime linux-x64 --self-contained false --verbosity normal

# Verify runtime config file exists
runtime_config=$(find ./publish -name "*.runtimeconfig.json" | head -n 1)
if [ -n "$runtime_config" ]; then
    echo "? Runtime configuration file found: $(basename "$runtime_config")"
else
    echo "? Warning: Runtime configuration file not found!"
    echo "Creating runtime configuration file..."
    
    # Create a basic runtime configuration file
    cat > ./publish/StargateAPI.runtimeconfig.json << 'EOF'
{
  "runtimeOptions": {
    "tfm": "net8.0",
    "framework": {
      "name": "Microsoft.AspNetCore.App",
      "version": "8.0.0"
    },
    "configProperties": {
      "System.GC.Server": true
    }
  }
}
EOF
    echo "? Runtime configuration file created: StargateAPI.runtimeconfig.json"
fi

# Copy configuration files
echo "Copying configuration files..."
cp -r .ebextensions ./publish/.ebextensions

# List published files for verification
echo "Published files:"
find ./publish -type f -exec basename {} \; | sort

# Create deployment package
echo "Creating deployment package..."
cd publish
zip -r ../stargate-api-deployment.zip .
cd ..

package_size=$(du -h stargate-api-deployment.zip | cut -f1)
echo "Deployment package created: stargate-api-deployment.zip ($package_size)"
echo "Ready for Elastic Beanstalk deployment!"
echo ""
echo "Next steps:"
echo "1. Run: eb deploy"
echo "2. Or upload stargate-api-deployment.zip via AWS Console"