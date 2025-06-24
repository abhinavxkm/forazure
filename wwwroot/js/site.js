$(document).ready(function () {
    // --- Navbar Scroll Effect ---
    var mainNavbar = $('#main-navbar');
    if (mainNavbar.hasClass('navbar-transparent')) {
        $(window).on('scroll', function () {
            if ($(window).scrollTop() > 50) {
                mainNavbar.addClass('scrolled');
            } else {
                mainNavbar.removeClass('scrolled');
            }
        });
    }

    // --- Centralized "Add to Cart" AJAX Functionality ---
    updateCartBadge();

    // Attach click handler to any button with the 'add-to-cart-btn' class on any page
    $(document).on('click', '.add-to-cart-btn', function (e) {
        e.preventDefault();
        var button = $(this);
        var propertyId = button.data('property-id');

        // Find the anti-forgery token from a form on the page
        var token = $('input[name="__RequestVerificationToken"]').val();

        // If the token is missing, we cannot proceed.
        if (!token) {
            showToast('Security token not found. Please refresh the page.', true);
            return;
        }

        $.ajax({
            type: 'POST',
            url: '/Buyer/AddToCart',
            data: {
                propertyId: propertyId
            },
            headers: {
                "RequestVerificationToken": token
            },
            success: function (response) {
                if (response.success) {
                    updateCartBadge();
                    showToast(response.message); // Use the custom toast for success
                    button.prop('disabled', true).text('Added!');
                } else {
                    showToast(response.message || 'Failed to add property.', true);
                }
            },
            error: function () {
                showToast('An error occurred. Please try again.', true);
            }
        });
    });
});

// --- Helper Functions ---

function updateCartBadge() {
    $.get('/Buyer/GetCartCount', function (response) {
        updateBadgeDisplay(response.count);
    }).fail(function () {
        updateBadgeDisplay(0);
    });
}

function updateBadgeDisplay(count) {
    var badge = $('#cart-badge');
    if (badge.length > 0) {
        if (count > 0) {
            badge.text(count).show();
        } else {
            badge.hide();
        }
    }
}

// Custom "toast" notification function
function showToast(message, isError = false) {
    $('.toast-message').remove();
    var toast = $('<div class="toast-message"></div>').text(message);
    if (isError) {
        toast.addClass('error');
    }
    $('body').append(toast);

    setTimeout(function () {
        toast.fadeOut(500, function () {
            $(this).remove();
        });
    }, 3000);
}
