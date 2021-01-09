

// check if an element exists in array using a comparer function
// comparer : function(currentElement)
Array.prototype.inArray = function(comparer) {
    for(var i=0; i < this.length; i++) {
        if(comparer(this[i])) return true;
    }
    return false;
};

// adds an element to the array if it does not already exist using a comparer 
// function
Array.prototype.pushIfNotExist = function(element, comparer) {
    if (!this.inArray(comparer)) {
        this.push(element);
    }
};

var knappen = new function() {

	var accessToken = 'pk.eyJ1IjoicGVwcGVsb3J1bSIsImEiOiJja2l4OWtpMWExMTJqMnNtZWNzNm03c2xuIn0.pANrn4v57gAfihydlYb_Sg';
	var map = L.map('mapid');
	var loading = false;
	var items = [];

	L.tileLayer('https://api.mapbox.com/styles/v1/{id}/tiles/{z}/{x}/{y}?access_token={accessToken}', {
		attribution: 'Map data &copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors, Imagery © <a href="https://www.mapbox.com/">Mapbox</a>',
		maxZoom: 18,
		id: 'mapbox/streets-v11',
		tileSize: 512,
		zoomOffset: -1,
		accessToken: accessToken
	}).addTo(map);

	var icons = {
		green: new L.Icon({
			iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-green.png',
			shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
			iconSize: [25, 41],
			iconAnchor: [12, 41],
			popupAnchor: [1, -34],
			shadowSize: [41, 41]
		}),
		blue: new L.Icon({
			iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-blue.png',
			shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
			iconSize: [25, 41],
			iconAnchor: [12, 41],
			popupAnchor: [1, -34],
			shadowSize: [41, 41]
		})
	}

	//Initial load of location
	function onLocationFound(e) {
		console.log('onLocationFound', e);
		var radius = e.accuracy;

		map.setView([e.latlng.lat, e.latlng.lng], 19);
		L.marker(e.latlng).addTo(map).bindPopup("You are within " + radius + " meters from this point").openPopup();

		var a = L.circle(e.latlng, radius).addTo(map);

		// setTimeout(() => {
		// 	map.removeLayer(a)
		// 	L.circle(e.latlng, radius).addTo(map);
		// }, 3000)

		// console.log('hej', e.latlng)

		loadItems(e.latlng.lng, e.latlng.lat);

		// document.querySelector('#lng').value = e.latlng.lng;
		// document.querySelector('#lat').value = e.latlng.lat;
	}

	function onLocationError(e) {
		alert(e.message);
	}

	// Event for on pan end
	function onMoveEnd(e) {
		// console.log('onMoveEnd');
		var location = e.target._lastCenter;

		loadItems(location.lng, location.lat)
	}

	function drawMarkers() {
		var markerClusterGroup = L.markerClusterGroup();

		items.forEach(item => {
			var icon = icons.blue;
			if (item.image != null) {
				icon = icons.green;
			}
			var marker = L.marker([item.lat, item.long], {icon: icon});
			if (item.image != null) {
				marker.bindPopup('<img class="popup" width="600" src="'+ item.image +'" />', { maxWidth: 600}).openPopup();
			}
			markerClusterGroup.addLayer(marker);
		});

		map.addLayer(markerClusterGroup);
	}


	function loadItems(lng, lat) {

		if (loading) return;
		loading = true;

		var center = map.getCenter();
		var eastBound = map.getBounds().getEast();
		var centerEast = L.latLng(center.lat, eastBound);
		var radius = center.distanceTo(centerEast);

		// var b = L.circle(center, radius).addTo(map);
		// var radius = dist * 0.75

		var url = '/api/items/nearby/?lng='+ lng +'&lat='+ lat +'&radius='+ radius;

		axios.get(url)
		.then(function (response) {
			for (let i = 0; i < response.data.length; i++) {
				var element = response.data[i];

				items.pushIfNotExist(element, function(e) { 
					return e.id === element.id;
				});
			}
			drawMarkers();
		})
		.catch(function (error) {
				// handle error
				console.log(error);
		})
		.then(function () {
				// always executed
				loading = false;
		});
	}

	map.on('moveend', onMoveEnd);
	map.on('locationfound', onLocationFound);
	map.on('locationerror', onLocationError);
	map.locate({setView: true, maxZoom: 16});

	function eventHandlers() {

		// return;

		document.querySelector('button').addEventListener('click', (e) => {
			e.preventDefault();
			var lng = document.querySelector('#lng').value;
			var lat = document.querySelector('#lat').value;

			var bodyFormData = new FormData();
			bodyFormData.append('long', lng);
			bodyFormData.append('lat', lat);

			axios({
				method: 'post',
				url: '/api/items/',
				data: bodyFormData,
				headers: {'Content-Type': 'multipart/form-data' }
			})
			.then(function (response) {
				//handle success
				console.log(response);
			})
			.catch(function (response) {
				//handle error
				console.log(response);
			});

			// axios.post('/api/items',
			// {
			// long: lng,
			// lat: lat
			// })
			// .then(function (response) {
			// 	// handle success
			// 	console.log('response', response.data);
			// 	alert('ok!')

			// 	// response.data.forEach(item => {
			// 	// 	// console.log('i', item)
			// 	// 	drawMarker(item);
			// 	// });
			// })
			// .catch(function (error) {
			// 		// handle error
			// 		console.log(error);
			// })
			// .then(function () {
			// 		// always executed
			// });
		})
	}

	this.onLoad = function() {
		// eventHandlers();
	};
}();


document.addEventListener('DOMContentLoaded', () => {
	knappen.onLoad();
})


