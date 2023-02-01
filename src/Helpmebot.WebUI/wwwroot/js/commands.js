var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl)
})


function commandSearch(){
    const searchBox = document.getElementById('search');
    var searchItem = searchBox.value.toLowerCase();

    if(searchBox.value === "") {
        const {
            host, hostname, href, origin, pathname, port, protocol, search
        } = window.location

        history.replaceState({}, "", pathname);
    } else {
        history.replaceState({}, "", "?q=" + searchBox.value.toLowerCase())
    }
    
    var shownCommands = [];
    var groups = {};
    $('#commandHelpItems .command-help-object').each(function (index) {
        var text = $(this).text().toLowerCase();

        if(text.indexOf(searchItem) > -1){
            $(this).css('display', '');
            shownCommands.push($(this).data('hmb-command'));
            groups[$(this).data('hmb-group')] = true;
        } else {
            $(this).css('display', 'none');
            groups[$(this).data('hmb-group')] = groups[$(this).data('hmb-group')] || false;
        }
    });
    
    $('.hmb-masonry-reflow').each(function (index) {
        $(this).data('masonry').layout();
    })
    
    $('.commandlist-toc-item').each(function(index) {
        if(shownCommands.includes($(this).data('hmb-command'))) {
            $(this).find('a').removeClass('text-decoration-none');
            $(this).find('a').removeClass('text-muted');
        } else {
            $(this).find('a').addClass('text-decoration-none');
            $(this).find('a').addClass('text-muted');
        }
    });
    
    $('.commandlist-toc-group').each(function(index) {
        if(groups[$(this).data('hmb-group')]) {
            $(this).find('a').removeClass('disabled');
            $(this).find('a').removeClass('text-muted');
            $(this).find('a').removeClass('text-decoration-none');
        } else {
            $(this).find('a').addClass('disabled');
            $(this).find('a').addClass('text-muted');
            $(this).find('a').addClass('text-decoration-none');
        }
    });
    
    $('.hmb-group-header').each(function(index){
       if(groups[$(this).data('hmb-group')]) {
           $(this).removeClass('d-none');
       } else {
           $(this).addClass('d-none');
       }
    });
}

(function($) {
    $(document).ready(function() {
        commandSearch()
    });
})(jQuery);