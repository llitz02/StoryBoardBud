// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// ============================================================================
// AJAX Auto-Save Functionality for Storyboards
// ============================================================================

/**
 * Auto-save manager for storyboard items.
 * Debounces save operations to prevent excessive server requests.
 */
const AutoSave = {
    timers: {},
    saveDelay: 1000, // 1 second delay before auto-saving
    
    /**
     * Schedules an auto-save operation with debouncing.
     * @@param {string} key - Unique identifier for the save operation
     * @@param {Function} saveFunction - The async function to execute
     */
    schedule(key, saveFunction) {
        // Clear existing timer for this key
        if (this.timers[key]) {
            clearTimeout(this.timers[key]);
        }
        
        // Schedule new save
        this.timers[key] = setTimeout(async () => {
            try {
                await saveFunction();
                this.showSaveIndicator('success');
            } catch (error) {
                console.error('Auto-save failed:', error);
                this.showSaveIndicator('error');
            }
        }, this.saveDelay);
        
        this.showSaveIndicator('saving');
    },
    
    /**
     * Displays save status indicator to user.
     * @@param {string} status - 'saving', 'success', or 'error'
     */
    showSaveIndicator(status) {
        let indicator = document.getElementById('autoSaveIndicator');
        
        if (!indicator) {
            indicator = document.createElement('div');
            indicator.id = 'autoSaveIndicator';
            indicator.setAttribute('role', 'status');
            indicator.setAttribute('aria-live', 'polite');
            indicator.setAttribute('aria-atomic', 'true');
            indicator.style.cssText = `
                position: fixed;
                top: 20px;
                right: 20px;
                padding: 10px 20px;
                border-radius: 4px;
                font-size: 14px;
                z-index: 9999;
                transition: opacity 0.3s;
            `;
            document.body.appendChild(indicator);
        }
        
        switch(status) {
            case 'saving':
                indicator.textContent = '💾 Saving...';
                indicator.style.backgroundColor = '#ffc107';
                indicator.style.color = '#000';
                break;
            case 'success':
                indicator.textContent = '✓ Saved';
                indicator.style.backgroundColor = '#28a745';
                indicator.style.color = '#fff';
                setTimeout(() => {
                    indicator.style.opacity = '0';
                    setTimeout(() => indicator.style.opacity = '1', 2000);
                }, 1500);
                break;
            case 'error':
                indicator.textContent = '✗ Save failed';
                indicator.style.backgroundColor = '#dc3545';
                indicator.style.color = '#fff';
                break;
        }
    }
};

/**
 * Auto-saves storyboard item position after drag operation.
 * @@param {string} itemId - GUID of the board item
 * @@param {number} posX - X coordinate position
 * @@param {number} posY - Y coordinate position
 * @@param {number} width - Item width in pixels
 * @@param {number} height - Item height in pixels
 * @@param {number} rotation - Rotation angle in degrees
 * @@param {number} zIndex - Z-index for layering
 */
function autoSaveItemPosition(itemId, posX, posY, width, height, rotation, zIndex) {
    AutoSave.schedule(`item-${itemId}`, async () => {
        const response = await fetch('/api/photos/update-item', {
            method: 'PUT',
            headers: { 
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify({
                itemId,
                posX,
                posY,
                width,
                height,
                rotation,
                zIndex
            })
        });
        
        if (!response.ok) {
            throw new Error('Failed to save item position');
        }
    });
}

/**
 * Auto-saves text content changes for board items.
 * @@param {string} itemId - GUID of the board item
 * @@param {string} textContent - Updated text content
 */
function autoSaveTextContent(itemId, textContent) {
    AutoSave.schedule(`text-${itemId}`, async () => {
        const response = await fetch('/api/photos/update-text', {
            method: 'PUT',
            headers: { 
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify({
                itemId,
                textContent
            })
        });
        
        if (!response.ok) {
            throw new Error('Failed to save text content');
        }
    });
}

// ============================================================================
// AJAX Form Submission Without Page Reload
// ============================================================================

/**
 * Handles form submission via AJAX with validation and error handling.
 * @@param {HTMLFormElement} form - The form element to submit
 * @@param {Object} options - Configuration options
 * @@param {Function} options.onSuccess - Callback on successful submission
 * @@param {Function} options.onError - Callback on error
 * @@param {boolean} options.showLoader - Whether to show loading indicator
 */
async function submitFormAjax(form, options = {}) {
    const {
        onSuccess = () => {},
        onError = (error) => alert('Error: ' + error),
        showLoader = true
    } = options;
    
    // Prevent default form submission
    event?.preventDefault();
    
    // Get form data
    const formData = new FormData(form);
    const url = form.action;
    const method = form.method || 'POST';
    
    // Show loading indicator
    let loader;
    if (showLoader) {
        loader = showFormLoader(form);
    }
    
    try {
        const response = await fetch(url, {
            method: method,
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        });
        
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(errorText || 'Form submission failed');
        }
        
        const result = await response.json();
        
        // Hide loader
        if (loader) hideFormLoader(loader);
        
        // Call success callback
        onSuccess(result);
        
    } catch (error) {
        // Hide loader
        if (loader) hideFormLoader(loader);
        
        // Call error callback
        onError(error.message);
    }
}

/**
 * Shows a loading indicator on the form.
 * @@param {HTMLFormElement} form - The form element
 * @@returns {HTMLElement} The loader element
 */
function showFormLoader(form) {
    const loader = document.createElement('div');
    loader.className = 'form-loader-overlay';
    loader.innerHTML = `
        <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
        </div>
    `;
    loader.style.cssText = `
        position: absolute;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(255, 255, 255, 0.8);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 1000;
    `;
    
    form.style.position = 'relative';
    form.appendChild(loader);
    
    // Disable form inputs
    const inputs = form.querySelectorAll('input, textarea, button, select');
    inputs.forEach(input => input.disabled = true);
    
    return loader;
}

/**
 * Hides the form loading indicator.
 * @@param {HTMLElement} loader - The loader element to remove
 */
function hideFormLoader(loader) {
    if (loader && loader.parentElement) {
        const form = loader.parentElement;
        
        // Re-enable form inputs
        const inputs = form.querySelectorAll('input, textarea, button, select');
        inputs.forEach(input => input.disabled = false);
        
        loader.remove();
    }
}

/**
 * Submits board creation form via AJAX without page reload.
 * @@param {Event} event - Form submit event
 */
async function submitCreateBoardAjax(event) {
    event.preventDefault();
    const form = event.target;
    
    await submitFormAjax(form, {
        onSuccess: (result) => {
            // Show success message
            showNotification('Board created successfully!', 'success');
            
            // Redirect to edit page after short delay
            setTimeout(() => {
                window.location.href = result.redirectUrl || `/Boards/Edit/${result.id}`;
            }, 500);
        },
        onError: (error) => {
            showNotification('Failed to create board: ' + error, 'error');
        }
    });
}

/**
 * Submits board update form via AJAX without page reload.
 * @@param {Event} event - Form submit event
 */
async function submitUpdateBoardAjax(event) {
    event.preventDefault();
    const form = event.target;
    
    await submitFormAjax(form, {
        onSuccess: (result) => {
            showNotification('Board updated successfully!', 'success');
            
            // Update page title if changed
            if (result.title) {
                document.title = result.title;
                const headerTitle = document.querySelector('.card-header h5');
                if (headerTitle) {
                    headerTitle.textContent = result.title;
                }
            }
        },
        onError: (error) => {
            showNotification('Failed to update board: ' + error, 'error');
        }
    });
}

/**
 * Shows a toast notification to the user.
 * @@param {string} message - The message to display
 * @@param {string} type - 'success', 'error', 'info', or 'warning'
 */
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `alert alert-${type === 'error' ? 'danger' : type} alert-dismissible fade show`;
    notification.setAttribute('role', 'alert');
    notification.setAttribute('aria-live', 'assertive');
    notification.setAttribute('aria-atomic', 'true');
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        min-width: 300px;
        box-shadow: 0 4px 6px rgba(0,0,0,0.1);
    `;
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close notification"></button>
    `;
    
    document.body.appendChild(notification);
    
    // Auto-remove after 5 seconds
    setTimeout(() => {
        notification.classList.remove('show');
        setTimeout(() => notification.remove(), 150);
    }, 5000);
}

// ============================================================================
// Event Listeners & Initialization
// ============================================================================

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    
    // Auto-save for text content editing
    document.querySelectorAll('.text-item[contenteditable="true"]').forEach(textItem => {
        textItem.addEventListener('input', function() {
            const boardItem = this.closest('.board-item');
            const itemId = boardItem.dataset.id;
            const textContent = this.textContent;
            
            autoSaveTextContent(itemId, textContent);
        });
    });
    
    // AJAX form submission for board creation
    const createBoardForm = document.querySelector('form[action*="Create"]');
    if (createBoardForm && !createBoardForm.dataset.ajaxEnabled) {
        createBoardForm.dataset.ajaxEnabled = 'true';
        createBoardForm.addEventListener('submit', submitCreateBoardAjax);
    }
    
    // AJAX form submission for board updates
    const updateBoardForm = document.querySelector('form[action*="Update"]');
    if (updateBoardForm && !updateBoardForm.dataset.ajaxEnabled) {
        updateBoardForm.dataset.ajaxEnabled = 'true';
        updateBoardForm.addEventListener('submit', submitUpdateBoardAjax);
    }
    
    // Keyboard navigation for board items
    initializeKeyboardNavigation();
});

// ============================================================================
// Keyboard Navigation Support for Accessibility
// ============================================================================

/**
 * Initializes keyboard navigation for board items.
 * Allows users to move and manipulate items using keyboard.
 */
function initializeKeyboardNavigation() {
    const boardItems = document.querySelectorAll('.board-item');
    
    boardItems.forEach(item => {
        item.addEventListener('keydown', function(e) {
            handleBoardItemKeydown(e, this);
        });
    });
    
    // Add keyboard support for canvas focus
    const canvas = document.getElementById('boardCanvas');
    if (canvas) {
        canvas.addEventListener('keydown', function(e) {
            if (e.key === '?' && e.shiftKey) {
                showKeyboardShortcuts();
            }
        });
    }
}

/**
 * Handles keyboard events for board items.
 * @@param {KeyboardEvent} e - The keyboard event
 * @@param {HTMLElement} item - The board item element
 */
function handleBoardItemKeydown(e, item) {
    const itemId = item.dataset.id;
    const step = e.shiftKey ? 10 : 1; // Shift key for larger movements
    
    switch(e.key) {
        case 'ArrowUp':
            e.preventDefault();
            moveBoardItem(item, 0, -step);
            break;
        case 'ArrowDown':
            e.preventDefault();
            moveBoardItem(item, 0, step);
            break;
        case 'ArrowLeft':
            e.preventDefault();
            moveBoardItem(item, -step, 0);
            break;
        case 'ArrowRight':
            e.preventDefault();
            moveBoardItem(item, step, 0);
            break;
        case 'Delete':
        case 'Backspace':
            e.preventDefault();
            if (confirm('Delete this item?')) {
                deleteItem(itemId);
            }
            break;
        case '[':
            e.preventDefault();
            sendBackward(itemId);
            break;
        case ']':
            e.preventDefault();
            bringForward(itemId);
            break;
        case 'Enter':
            // Allow Enter for text editing
            if (!item.querySelector('.text-item[contenteditable="true"]')) {
                e.preventDefault();
            }
            break;
    }
}

/**
 * Moves a board item by the specified offset.
 * @@param {HTMLElement} item - The board item to move
 * @@param {number} deltaX - Horizontal movement in pixels
 * @@param {number} deltaY - Vertical movement in pixels
 */
function moveBoardItem(item, deltaX, deltaY) {
    const currentLeft = parseFloat(item.style.left) || 0;
    const currentTop = parseFloat(item.style.top) || 0;
    
    const newLeft = Math.max(0, currentLeft + deltaX);
    const newTop = Math.max(0, currentTop + deltaY);
    
    item.style.left = newLeft + 'px';
    item.style.top = newTop + 'px';
    
    // Auto-save position
    if (typeof autoSaveItemPosition === 'function') {
        const itemId = item.dataset.id;
        const currentZIndex = parseInt(item.dataset.zindex || 0);
        
        autoSaveItemPosition(
            itemId,
            newLeft,
            newTop,
            item.offsetWidth,
            item.offsetHeight,
            0,
            currentZIndex
        );
    }
}

/**
 * Shows keyboard shortcuts help dialog.
 */
function showKeyboardShortcuts() {
    const helpText = `
        Keyboard Shortcuts:
        
        Arrow Keys: Move item (Hold Shift for 10px)
        [ : Send backward
        ] : Bring forward
        Delete/Backspace: Delete item
        Shift + ?: Show this help
    `;
    
    alert(helpText);
}
