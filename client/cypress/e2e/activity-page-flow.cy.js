describe('Activity Page User Flow', () => {
  beforeEach(() => {
    // Visit the home page before each test
    cy.visit('/')
  })

  it('should navigate from home page to activity page', () => {
    // Wait for activities to load
    cy.contains('Activities', { timeout: 10000 }).should('be.visible')

    // Find and click on the first activity's "Start Activity" button
    cy.get('[data-cy="start-activity-button"]').first().click()

    // Should navigate to activity page
    cy.url().should('include', '/activity/')

    // Activity page should load
    cy.get('[data-cy="activity-header"]').should('be.visible')
  })

  it('should load activity data successfully', () => {
    // Navigate to activity page
    cy.get('[data-cy="start-activity-button"]').first().click()

    // Wait for activity data to load
    cy.get('[data-cy="activity-title"]').should('be.visible')
    cy.get('[data-cy="activity-description"]').should('be.visible')

    // Progress bar should be visible
    cy.get('[data-cy="progress-bar"]').should('be.visible')

    // Video player should be present (if video exists)
    cy.get('video').should('exist')
  })

  it('should display activity steps and allow navigation', () => {
    // Navigate to activity page
    cy.get('[data-cy="start-activity-button"]').first().click()

    // Wait for steps to load
    cy.get('[data-cy="step-navigator"]').should('be.visible')

    // Should have at least one step
    cy.get('[data-cy="step-button"]').should('have.length.greaterThan', 0)

    // First step should be active
    cy.get('[data-cy="step-button"]').first().should('have.class', 'active')

    // Click on a different step
    cy.get('[data-cy="step-button"]').eq(1).click()

    // That step should become active
    cy.get('[data-cy="step-button"]').eq(1).should('have.class', 'active')
  })

  it('should synchronize video playback with steps', () => {
    // Navigate to activity page
    cy.get('[data-cy="start-activity-button"]').first().click()

    // Wait for video to load
    cy.get('video').should('be.visible')

    // Start playing video
    cy.get('video').then($video => {
      $video[0].play()
    })

    // Video should be playing
    cy.get('video').should('have.prop', 'paused', false)

    // When video reaches step pause time, it should pause and show step overlay
    // This would require mocking video timing or using a test video with known timestamps
    cy.wait(2000) // Wait for potential pause

    // Check if step overlay appears (if video has pause points)
    cy.get('[data-cy="step-overlay"]').should('exist')
  })

  it('should allow continuing through steps', () => {
    // Navigate to activity page
    cy.get('[data-cy="start-activity-button"]').first().click()

    // Click continue on step overlay if present
    cy.get('[data-cy="continue-button"]').click()

    // Video should resume playing
    cy.get('video').should('have.prop', 'paused', false)

    // Progress should update
    cy.get('[data-cy="progress-bar"]').should('contain', '33') // Assuming 3 steps
  })

  it('should open and display materials dialog', () => {
    // Navigate to activity page
    cy.get('[data-cy="start-activity-button"]').first().click()

    // Click materials button
    cy.get('[data-cy="materials-button"]').click()

    // Materials dialog should open
    cy.get('[data-cy="materials-dialog"]').should('be.visible')

    // Should display materials list
    cy.get('[data-cy="material-item"]').should('have.length.greaterThan', 0)

    // Close dialog
    cy.get('[data-cy="close-materials-dialog"]').click()

    // Dialog should close
    cy.get('[data-cy="materials-dialog"]').should('not.be.visible')
  })

  it('should track progress through activity', () => {
    // Navigate to activity page
    cy.get('[data-cy="start-activity-button"]').first().click()

    // Initial progress should be 0%
    cy.get('[data-cy="progress-bar"]').should('contain', '0')

    // Complete first step
    cy.get('[data-cy="continue-button"]').click()

    // Progress should update
    cy.get('[data-cy="progress-bar"]').should('not.contain', '0')

    // Navigate to next step
    cy.get('[data-cy="step-button"]').eq(1).click()

    // Progress should continue to update
    cy.get('[data-cy="progress-bar"]').invoke('text').then(text => {
      const progress = parseInt(text.replace('%', ''))
      expect(progress).to.be.greaterThan(0)
    })
  })

  it('should be responsive on mobile screens', () => {
    // Set viewport to mobile
    cy.viewport('iphone-6')

    // Navigate to activity page
    cy.get('[data-cy="start-activity-button"]').first().click()

    // Activity page should adapt to mobile layout
    cy.get('[data-cy="activity-header"]').should('be.visible')

    // Video should be appropriately sized for mobile
    cy.get('video').should('be.visible')

    // Steps should be navigable on mobile
    cy.get('[data-cy="step-navigator"]').should('be.visible')

    // Materials button should be accessible
    cy.get('[data-cy="materials-button"]').should('be.visible')
  })

  it('should be responsive on tablet screens', () => {
    // Set viewport to tablet
    cy.viewport('ipad-2')

    // Navigate to activity page
    cy.get('[data-cy="start-activity-button"]').first().click()

    // Activity page should adapt to tablet layout
    cy.get('[data-cy="activity-header"]').should('be.visible')

    // Layout should be optimized for tablet
    cy.get('[data-cy="activity-layout"]').should('be.visible')
  })

  it('should handle navigation back to activities', () => {
    // Navigate to activity page
    cy.get('[data-cy="start-activity-button"]').first().click()

    // Click back button
    cy.get('[data-cy="back-button"]').click()

    // Should navigate back to home page
    cy.url().should('eq', Cypress.config('baseUrl') + '/')

    // Activities should be visible again
    cy.contains('Activities').should('be.visible')
  })

  it('should handle activity completion', () => {
    // Navigate to activity page
    cy.get('[data-cy="start-activity-button"]').first().click()

    // Progress through all steps
    cy.get('[data-cy="step-button"]').each(($step, index) => {
      cy.wrap($step).click()
      if (index < cy.get('[data-cy="step-button"]').length - 1) {
        cy.get('[data-cy="continue-button"]').click()
      }
    })

    // Progress should be 100%
    cy.get('[data-cy="progress-bar"]').should('contain', '100')

    // Video should end
    cy.get('video').should('have.prop', 'ended', true)
  })
})