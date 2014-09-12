﻿(function () {
    angular.module('services')
        .factory('flatTaxonService', ['$resource', function ($resource) {
            /* Private methods and variables */
            var getResource = function (taxonomyId, taxonId) {
                var url = sitefinity.services.getFlatTaxonServiceUrl();
                if (taxonomyId && taxonomyId !== "") {
                    url = url + taxonomyId + '/';

                    if (taxonId && taxonId !== "") {
                        url = url + taxonId + '/';
                    }
                }
                return $resource(url);
            };

            var dataItemPromise;

            var getTaxons = function (taxonomyId, provider, skip, take, filter) {
                var generatedFilter;
                if (filter) {
                    generatedFilter = '(Title.ToUpper().Contains("' + filter + '".ToUpper()))';
                }

                dataItemPromise = getResource(taxonomyId).get(
                    {
                        provider: provider,
                        sortExpression: 'Title ASC',
                        skip: skip,
                        take: take,
                        filter: generatedFilter
                    })
                    .$promise;

                return dataItemPromise;
            };

            var getTaxon = function (taxonomyId, taxonId, provider) {
                dataItemPromise = getResource(taxonomyId, taxonId).get(
                    {
                        provider: provider
                    })
                    .$promise;

                return dataItemPromise;
            };

            return {
                /* Returns the data items. */
                getTaxons: getTaxons,
                getTaxon: getTaxon
            };
        }]);
})();