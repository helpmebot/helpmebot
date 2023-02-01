function genericSearch(){
    var searchBox = document.getElementById('search');
    var searchItems = document.getElementById('searchTarget').getElementsByTagName('tr');
    
    if(searchBox.value === "") {
        const {
            host, hostname, href, origin, pathname, port, protocol, search
        } = window.location
        
        history.replaceState({}, "", pathname);    
    } else {
        history.replaceState({}, "", "?q=" + searchBox.value.toLowerCase())
    }
    

    for(i = 0; i<searchItems.length; i++) {
        var text = searchItems[i].textContent.toLowerCase();
        if(text.indexOf(searchBox.value.toLowerCase()) > -1){
            searchItems[i].style.display = ""
        } else {
            searchItems[i].style.display = "none"
        }
    }
}

(function($) {
    $(document).ready(function() {
        genericSearch()
    });
})(jQuery);