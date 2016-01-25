(function () {
    var app = angular.module("carFinderApp", ['trNgGrid', 'ui.bootstrap']);
    app.controller('carCtrl', ['carDataSvc', '$uibModal', function (carDataSvc, $uibModal) { //['carDataSvc', ...
        var scope = this;
        scope.years = [];
        scope.makes = [];
        scope.models = [];
        scope.trims = [];
        scope.selected = {
            year: '',
            make: '',
            model: '',
            trim: ''
        }
        scope.cars = [];
        scope.id = '';

        //scope.viewCar = function (id) {
        //    console.log('in viewCar', id)
        //    carDataSvc.getCarDetail(scope.id).then(function (data) {
        //        scope.cars = data;
        //    })

        //}

        scope.getCars = function() {
            carDataSvc.getCars(scope.selected).then(function (data) {
                scope.cars = data;
            })
        }

        scope.getYears = function () {
            carDataSvc.getYears().then(function (data) { //picks up data and throws it into scope.years
                scope.years = data;
                scope.makes = [];
                scope.models = [];
                scope.trims = [];
                scope.selected.year = '';
                scope.selected.make = '';
                scope.selected.model = '';
                scope.selected.trim = '';

            })
        }

        scope.getMakes = function () {
            carDataSvc.getMakes(scope.selected).then(function (data) { 
                scope.makes = data;
                scope.models = [];
                scope.trims = [];
                scope.selected.make = '';
                scope.selected.model = '';
                scope.selected.trim = '';
                scope.getCars();



            })
        }

        scope.getModels = function () {
            carDataSvc.getModels(scope.selected).then(function (data) { 
                scope.models = data;
                scope.trims = [];
                scope.selected.model = '';
                scope.selected.trim = '';
                scope.getCars();

            })
        }

        scope.getTrims = function () {
            carDataSvc.getTrims(scope.selected).then(function (data) {
                scope.trims = data;
                scope.selected.trim = '';
                scope.getCars();

            })
        }


        scope.getYears();

        scope.open = function (id) {
            console.log("Id in open " + id)
            var modalInstance = $uibModal.open({
                animation: true,
                templateUrl: 'carModal.html',
                controller: 'carModalCtrl as cm',
                size: 'lg',
                resolve: {
                    car: function() { 
                        return carDataSvc.getCarDetail(id)
                    }
                 }
            });
        }
    }]);
    app.controller('carModalCtrl', function ($uibModalInstance, car) { // add car later to params

        var scope = this;
        scope.n = 0;
        scope.car = car;

        scope.ok = function () {
            $uibModalInstance.close();
        };

        scope.cancel = function () {
            $uibModalInstance.dismiss();
        };
    })
})();