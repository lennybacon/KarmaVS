(
    function () {
        if (!this['describe']) {
            this.describe =
                function (title, func) {
                    /// <summary>Defines a suite of specifications.</summary>
                    /// <param name="title" type="String">The suite title.</param>
                    /// <param name="func" type="Function">The suite function.</param>


            };
        }
        if (!this['xdescribe']) {
            this.xdescribe =
                function (title, func) {
                    /// <summary>Defines a disabled suite of specifications.</summary>
                    /// <param name="title" type="String">The suite title.</param>
                    /// <param name="func" type="Function">The suite function.</param>


                };
        }
        if (!this['it']) {
            this.it =
                function (title, func) {
                    /// <summary>Creates a Jasmine specification that will be added to the current suite.</summary>
                    /// <param name="title" type="String">The specification title.</param>
                    /// <param name="func" type="Function">The specification function.</param>


                };
        }
        if (!this['xit']) {
            this.xit =
                function (title, func) {
                    /// <summary>Creates a disabled Jasmine specification.</summary>
                    /// <param name="title" type="String">The specification title.</param>
                    /// <param name="func" type="Function">The specification function.</param>


                };
        }
        if (!this['spyOn']) {
            this.spyOn =
                function (obj, methodName) {
                    /// <summary>Installs a spy on an existing object's method name.  Used within a Spec to create a spy.</summary>
                    /// <param name="obj" type="Object">The specification title.</param>
                    /// <param name="methodName" type="String">The specification function.</param>
                    var spy = {                        
                        
                    };
                    return spy;

                };
        }
        if (!this['beforeEach']) {
            this.beforeEach =
                function (func) {
                    /// <summary>A function that is called before each spec in a suite.</summary>
                    /// <param name="func" type="Function">The function.</param>
            };
        }
        if (!this['afterEach']) {
            this.afterEach =
                function (func) {
                    /// <summary>A function that is called after each spec in a suite.</summary>
                    /// <param name="func" type="Function">The function.</param>
                };
        }
        if (!this['expect']) {
            this.expect =
                function (value) {
                    /// <summary>evaluates a object in an expectation.</summary>
                    /// <param name="value" type="Object">The function.</param>

                    var matcher = {
                        toBe: function(expected) {
                            /// <summary>Compares the actual to the expected using ===</summary>
                            /// <param name="expected" type="Object"></param>
                        },
                        toNotBe: function(expected) {
                            /// <summary>Compares the actual to the expected using !==</summary>
                            /// <param name="expected" type="Object"></param>
                        },
                        toEqual: function(expected) {
                            /// <summary>Compares the actual to the expected using common sense equality. Handles Objects, Arrays, etc.</summary>
                            /// <param name="expected" type="Object"></param>
                        },
                        toNotEqual: function(expected) {
                            /// <summary>Inverses the comparsion of the actual to the expected using common sense equality. Handles Objects, Arrays, etc.</summary>
                            /// <param name="expected" type="Object"></param>
                        },
                        toMatch: function(expected) {
                            /// <summary>Compares the actual to the expected using a regular expression.  Constructs a RegExp, so takes a pattern or a String.</summary>
                            /// <param name="expected" type="Object"></param>
                        },
                        toBeDefined: function() {
                            /// <summary>Inverses the comparison of the actual to jasmine.undefined.</summary>

                        },
                        toBeUndefined: function() {
                            /// <summary>Compares the actual to jasmine.undefined.</summary>
                        },
                        toBeNull: function() {
                            /// <summary>Compares the actual to null.</summary>
                        },
                        toBeNaN: function() {
                            /// <summary>Compares the actual to NaN</summary>
                        },
                        toBeTruthy: function() {
                            /// <summary>Matcher that boolean not-nots the actual.</summary>
                        },
                        toBeFalsy: function() {
                            /// <summary>Matcher that boolean nots the actual.</summary>
                        },
                        toHaveBeenCalled: function () {
                            /// <summary>Matcher that checks to see if the actual, a Jasmine spy, was called.</summary>
                        },
                        wasCalled: function () {
                            /// <summary>Matcher that checks to see if the actual, a Jasmine spy, was called.</summary>
                        },
                        wasNotCalled: function () {
                            /// <summary>Matcher that checks to see if the actual, a Jasmine spy, was not called.</summary>
                        },
                        toHaveBeenCalledWith: function (args) {
                            /// <summary>Matcher that checks to see if the actual, a Jasmine spy, was called with a set of parameters.</summary>
                        },
                        wasCalledWith: function (args) {
                            /// <summary>Matcher that checks to see if the actual, a Jasmine spy, was called with a set of parameters.</summary>
                        },
                        wasNotCalledWith: function (args) {
                            /// <summary>Matcher that checks to see if the actual, a Jasmine spy, was not called with a set of parameters.</summary>
                        },
                        toContain: function (obj) {
                            /// <summary>Matcher that checks that the expected item is an element in the actual Array.</summary>
                        },
                        toNotContain: function (obj) {
                            /// <summary>Matcher that checks that the expected item is not an element in the actual Array.</summary>
                        },
                        toBeLessThan: function (expected) {
                            /// <summary>Matcher that checks that the expected is less than actual.</summary>
                        },
                        toBeGreaterThan: function (expected) {
                            /// <summary>Matcher that checks that the expected is greater than actual.</summary>
                        },
                        toBeCloseTo: function (expected, precision) {
                            /// <summary>Matcher that checks that the expected item is equal to the actual item up to a given level of decimal precision (default 2).</summary>
                        },
                        toThrow: function (expected, precision) {
                            /// <summary>Matcher that checks that the expected exception was thrown by the actual.</summary>
                        }
                    };

                    matcher.not = function() {
                        return matcher;
                    };
                    
                    
                    return matcher;
                };
        }

    }()
);

