'use strict';
(function () {
    angular.module('directiveExampleApp')
        .factory(
            'appModelDataService',
            [
                'appModel',
                function (appModel) {
                    appModel = appModel || {};

                    function getModelPart(model, path) {
                        var property,
                            subModel,
                            pos;
                        if (typeof model != 'object')
                        {
                            return {};
                        }
                        if (typeof path != 'string') {
                            return model;
                        }
                        if (path.length == 0) {
                            return model;
                        }
                        pos = path.indexOf('.');
                        if (pos < 0) {
                            return model[path];
                        }

                        property = path.substring(0, pos);
                        subModel = model[property];
                        path = path.substring(pos + 1, path.length);
                        return getModelPart(subModel, path);
                    }

                    return {
                        getModel: function (path) {
                            if (!appModel.model)
                                return {};
                            return getModelPart(appModel.model, path);
                        },
                        getPropertyState: function (propertyKey) {
                            var mi;
                            if (typeof propertyKey != 'string') {
                                return null;
                            }
                            if (!appModel.metaInformation) {
                                return null;
                            }
                            mi = appModel.metaInformation;
                            if (mi.propertyState == null) {
                                return null;
                            }
                            return mi.propertyState[propertyKey];
                        }
                    };
                }
            ]
        );
}());
