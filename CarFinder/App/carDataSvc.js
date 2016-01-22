(function(){
    angular.module('carFinderApp').factory('carDataSvc', ['$http', function ($http) {

    var carFinderSvc = {};
    
    carFinderSvc.getYears = function() {
        return $http.post('/api/car/GetUniqueYears').then (function (response) {
            return response.data;
         })
    }

    carFinderSvc.getMakes = function (selected) {
        return $http.post('/api/car/GetMakesByYear', selected).then(function (response) {
            return response.data;
        })
    }

    carFinderSvc.getModels = function (selected) {
        return $http.post('/api/car/GetModelsByYrMk', selected).then(function (response) {
            return response.data;
        })
    }

    carFinderSvc.getTrims = function (selected) {
        return $http.post('/api/car/GetTrimByYrMkMod', selected).then(function (response) {
            return response.data;
        })
    }

    carFinderSvc.getCars = function (selected) {
        return $http.post('/api/car/GetCarData', selected).then(function (response) {
            return response.data;
        })
    }

    carFinderSvc.getCarDetail = function (id) { 
        return $http.post('/api/car/GetCar', {id:id}).then(function (response) {
            return response.data;
        })
    }

    return carFinderSvc;
}]);
})();