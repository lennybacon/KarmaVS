#Visual Studio Extension for KarmaJS

## Prerequisites
- Install nodeJS from http://www.nodejs.org
- Install KarmaJS by running the command line 

	npm install -g karma
	
- Install Jasmine Testing Framework for KarmaJS by running the command line 

	npm install -g karma-jasmine
		
- Install JUnit Reporter for KarmaJS by running the command line 

	npm install -g karma-junit-reporter

	
- Install Angular Scenario Test Adapter for KarmaJS by running the command line 

	npm install -g karma-ng-scenario

- Install Angular HTML 2 JS  Preprocessor for KarmaJS by running the command 
  line 

	npm install -g karma-ng-html2js-preprocessor

- Install the VSIX built by your own or from 
  http://visualstudiogallery.msdn.microsoft.com/02f47876-0e7a-4f6c-93f8-1af5d5189225

## Let it roll
- Create a Web Project in visual Studio
- Create a Karma configuration file in the root directory of your web 
  Application
- From the file menu select "Tools > Enable/disable Karma"
- NOTE: When a solution is newly loaded KarmaVS automatically checks for a 
  karma.conf.js file - if found Karma is started automatically