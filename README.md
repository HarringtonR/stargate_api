# Overview

This repository contains a technical exercise that is to be used for reference during a technical interview.
Be prepared to discuss the exercise, the expected results and how you came by them.

## Tasks

1. Follow the instructions in the following folders.  
1. Share the completed exercise back with the person who shared this link with you. (how, is up to you)


## Deployment & Infrastructure

### Current AWS Deployment
The application is deployed and running on AWS using the following architecture:

**Backend API:**
- **Service**: AWS Elastic Beanstalk
- **Environment**: Production
- **URL**: `https://stargate-api-prod.eba-spvrrfv5.us-east-1.elasticbeanstalk.com`
- **Platform**: .NET 8 on Amazon Linux 2
- **Database**: SQLite (embedded with application)
- **Health Checks**: Configured for load balancer monitoring

**Frontend Application:**
- **Service**: AWS Amplify
- **URL**: `https://main.dqh6niin9ecfm.amplifyapp.com`
- **Build**: Angular 18 SPA with automatic deployments
- **CORS**: Configured to allow API communication

**Key Infrastructure Features:**
- Load balancer health checks on `/health` endpoint  
- Forwarded headers configuration for proxy support
- CORS policy allowing cross-origin requests between services
- Automatic database initialization on startup
- Centralized process logging stored in database

**Deployment Process:**
- API: Packaged and deployed via Elastic Beanstalk CLI/Console
- Frontend: Continuous deployment from Git repository via Amplify
- Environment-specific configuration managed through `appsettings.json`