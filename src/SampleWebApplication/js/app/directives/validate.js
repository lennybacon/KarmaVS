'use strict';

angular.module('directiveExampleApp')
	.directive(
	    'validate',
	    [
	        'appModelDataService',
	        function (appModelDataService) {
	           
	            function emitEvent(scope, type, modelpath) { //fn scope: directive scope
	                scope.$root.$emit(type + '_' + modelpath);
	            }

	            function updatePropertyState(scope, controller, propertyState) {
	                if (!propertyState) {
	                    return;
	                }

	                var value = controller.$viewValue,
                        valueIsEmpty = !value || value.length === 0,
                        valueHasChangedSinceInitial = scope.initalValue !== value,
	                    isRequired = scope.$eval(propertyState.required.expression);

	                propertyState.errors.splice(0, propertyState.errors.length);

	                // If nothing changed ...
	                if (!valueHasChangedSinceInitial &&
	                    // ... and NOT (a required field with an empty value)
	                    //  because if the initial value is '' and after some 
	                    //  editing it is '' it's invalid if it's a required field...
                        !(isRequired && valueIsEmpty)) {
	                    if (!scope.initalErrors) {
	                        return;
	                    }
	                    // Replay server errors...
	                    propertyState.errors.unshift(scope.initalErrors);
	                }

	                //always handle required
	                if (isRequired) {
	                    var indexOfRequiredMessage = propertyState.errors.indexOf(propertyState.required.message);
	                    if (valueIsEmpty && indexOfRequiredMessage === -1) {
	                        propertyState.errors.unshift(propertyState.required.message);
	                        return;
	                    } else if (indexOfRequiredMessage > -1) {
	                        propertyState.errors.splice(indexOfRequiredMessage, 1);
	                    }
	                }
	                
	            }


	            return {
	                restrict: 'A',
	                require: 'ngModel',
	                // scope = the parent scope
	                // elem = the element the directive is on
	                // attr = a dictionary of attributes on the element
	                // ctrl = the controller for ngModel.
	                link: function (scope, elem, attr, ctrl) {
	                    console.log('linking validate');
	                    //get the regex flags from the regex-validate-flags="" 
	                    //  attribute(optional)
	                    var modelpath = attr.validate,
                            propertyState, typeValidationType;

	                    if (!modelpath) {
	                        throw 'The validate directive can only be ' +
	                            'used with a modelpath provided within the ' +
	                            'validate attribute';
	                    }

	                    propertyState =
	                        appModelDataService.getPropertyState(modelpath);

	                    //initial
	                    if (propertyState) {
	                        typeValidationType =
	                            propertyState.typeValidation
	                                ? propertyState.typeValidation.type.toLowerCase()
	                                : undefined;

	                        if (propertyState.required && propertyState.required.expression)
	                            attr.$set('required', true);

	                        if (typeValidationType) {
	                            attr.$set('pattern', typeValidationType);
	                        }
	                    }

	                    // model to DOM
	                    ctrl.$formatters.unshift(
	                        function (value) {
	                            console.log('validate: $formatters.unshift');
	                            scope.initalValue = value;
	                            if (propertyState) {
	                                if (!!value && value.length > 0 &&
	                                    propertyState.errors.length === 1){
	                                    scope.initalErrors =
	                                        propertyState.errors[0];
	                                }
	                            }
	                            return value;
	                        }
	                    );

	                    // DOM to model
	                    ctrl.$parsers.unshift(
	                        function (value) {
	                            console.log('validate: $parsers.unshift');
	                            if (!propertyState) {
	                                return value;
	                            }

	                            if (propertyState.required) {
	                                ctrl.$setValidity(
	                                    'required',
	                                    value && value.length !== 0);
	                            }
	                            
	                            updatePropertyState(scope, ctrl, propertyState);

	                            emitEvent(scope, 'update', modelpath);
	                            return value;
	                        }
	                    );

	                    scope.focus =
	                        function(evt) {
	                            console.log('validate: focus');
	                            var evt1 = evt;
	                            scope.$apply(
	                                function() {
	                                    emitEvent(scope, evt1.type, modelpath);
	                                }
	                            );
	                        };
	                    
	                    //Every form element shall react on focus
	                    elem.bind('focus', scope.focus);

	                    scope.blur =
	                        function(evt) {
	                            console.log('validate: blur');
	                            var value = ctrl.$viewValue;
	                            emitEvent(scope, evt.type, modelpath);

	                            if (!propertyState) {
	                                return;
	                            }


	                            if (propertyState.required) {
	                                ctrl.$setValidity(
	                                    'required',
	                                    value && value.length !== 0);
	                            }

	                            updatePropertyState(scope, ctrl, propertyState);

	                            scope.$apply(
	                                function() {
	                                    emitEvent(scope, 'update', modelpath);
	                                }
	                            );
	                        };

	                    //Every form element shall react on blur
	                    elem.bind('blur', scope.blur);
	                    
	                    scope.$root.$on(
	                        'response_' + modelpath,
	                        function () {
	                            if (!propertyState) {
	                                return;
	                            }

	                            scope.initalValue = ctrl.$viewValue;
	                            scope.initalErrors = propertyState.errors[0];

	                            updatePropertyState(scope, ctrl, propertyState);

	                            emitEvent(scope, 'update', modelpath);
	                        }
	                    );

	                }
	            };
	        }
	    ]
	);