﻿; (function () {

    var selectors = angular.module('selectors');

    /*
     * The main controller of the Image Selector.
     */
    selectors.controller('sfImageSelectorCtrl', ['$scope', function ($scope) {

        $scope.cancel = function () {
            $scope.$modalInstance.close();
        };

        $scope.saveChanges = function () {

        };

    }]);

    /*
     * Image Selector Modal directive wraps the Image selector in a modal dialog. It is a 
     * convenience thin wrapper which can be used to open the Image Selector in a modal dialog.
     */
    selectors.directive('sfImageSelectorModal', ['$compile', 'serverContext', function ($compile, serverContext) {

        var link = function ($scope, element, attributes) {

            var selectorTemplate = serverContext.getEmbeddedResourceUrl('Telerik.Sitefinity.Frontend', 'Selectors/image-selector.html'),
                directiveMarkup = '<div modal existing-scope="true" window-class="sf-image-selector-dlg" template-url="' + selectorTemplate + '"></div>',
                modalDirective = $compile(directiveMarkup)($scope);

            // appends the compiled modal directive 
            var modalElement = $(element).append(modalDirective);

            /*
             * Opens the image selector modal dialog.
             */
            $scope.open = function () {
                angular.element(modalElement).scope().$openModalDialog();
            };

        };

        return {
            restrict: 'E',
            link: link
        };

    }]);

    /*
     * Image Selector Button directive is used together with the Image Selector Modal
     * for the cases when the Image Selector should be opened on the button click and
     * it should be open in a modal dialog.
     *
     * To use this directive set the the value of the sf-image-selector-button attribute
     * to jQuery selector of the sf-image-selector-modal element.
     */
    selectors.directive('sfImageSelectorButton', function () {

        var link = function ($scope, element, attributes) {

            var modal = attributes.sfImageSelectorButton;
            if (!modal) {
                throw 'You must specify the selector for the sf-image-selector-modal element.';
            }

            $(element).bind('click', function () {
                angular.element(modal).scope().open();
            });

        };

        return {
            restrict: 'A',
            link: link
        };

    });
        

}());