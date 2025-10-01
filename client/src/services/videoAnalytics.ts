interface VideoInteractionData {
  activityId: string;
  videoUrl: string;
  sessionId: string;
  timestamp: number;
  viewDuration: number;
  pauseDuration: number;
  scrollSpeed: number;
  mouseMovements: number;
  interactionCount: number;
  playCount: number;
  pauseCount: number;
  muteCount: number;
  unmuteCount: number;
  completionRate: number; // 0-1
  viewportTime: number; // Time video was in viewport
  engagementScore: number; // Calculated engagement score
}

interface UserPreferences {
  preferredCategories: string[];
  preferredDuration: number;
  preferredEngagementLevel: 'low' | 'medium' | 'high';
  autoplayPreference: boolean;
  mutePreference: boolean;
  scrollSpeed: number;
  interactionStyle: 'passive' | 'active' | 'engaged';
}

interface VideoAnalyticsConfig {
  enableTracking: boolean;
  sampleRate: number; // 0-1, percentage of events to track
  maxStorageSize: number; // Max localStorage size in KB
  flushInterval: number; // Flush interval in seconds
}

class VideoAnalyticsService {
  private config: VideoAnalyticsConfig = {
    enableTracking: true,
    sampleRate: 1.0,
    maxStorageSize: 1024, // 1MB
    flushInterval: 30, // 30 seconds
  };

  private sessionId: string;
  private userId: string;
  private interactionQueue: VideoInteractionData[] = [];
  private flushTimer: NodeJS.Timeout | null = null;
  private preferences: UserPreferences;

  constructor() {
    this.sessionId = this.generateSessionId();
    this.userId = this.getOrCreateUserId();
    this.preferences = this.loadUserPreferences();
    this.startAutoFlush();
  }

  // Generate unique session ID
  private generateSessionId(): string {
    return `session_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  // Get or create persistent user ID
  private getOrCreateUserId(): string {
    const stored = localStorage.getItem('funny_activities_user_id');
    if (stored) return stored;

    const newId = `user_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    localStorage.setItem('funny_activities_user_id', newId);
    return newId;
  }

  // Load user preferences from localStorage
  private loadUserPreferences(): UserPreferences {
    const stored = localStorage.getItem('funny_activities_preferences');
    if (stored) {
      try {
        return { ...this.getDefaultPreferences(), ...JSON.parse(stored) };
      } catch (e) {
        console.warn('Failed to parse user preferences:', e);
      }
    }
    return this.getDefaultPreferences();
  }

  // Get default user preferences
  private getDefaultPreferences(): UserPreferences {
    return {
      preferredCategories: [],
      preferredDuration: 300, // 5 minutes
      preferredEngagementLevel: 'medium',
      autoplayPreference: true,
      mutePreference: true,
      scrollSpeed: 0,
      interactionStyle: 'passive',
    };
  }

  // Save user preferences to localStorage
  private saveUserPreferences(): void {
    try {
      localStorage.setItem('funny_activities_preferences', JSON.stringify(this.preferences));
    } catch (e) {
      console.warn('Failed to save user preferences:', e);
    }
  }

  // Track video interaction
  public trackInteraction(data: Partial<VideoInteractionData>): void {
    if (!this.config.enableTracking) return;

    // Sample rate check
    if (Math.random() > this.config.sampleRate) return;

    const interactionData: VideoInteractionData = {
      activityId: data.activityId || 'unknown',
      videoUrl: data.videoUrl || '',
      sessionId: this.sessionId,
      timestamp: Date.now(),
      viewDuration: data.viewDuration || 0,
      pauseDuration: data.pauseDuration || 0,
      scrollSpeed: data.scrollSpeed || 0,
      mouseMovements: data.mouseMovements || 0,
      interactionCount: data.interactionCount || 0,
      playCount: data.playCount || 0,
      pauseCount: data.pauseCount || 0,
      muteCount: data.muteCount || 0,
      unmuteCount: data.unmuteCount || 0,
      completionRate: data.completionRate || 0,
      viewportTime: data.viewportTime || 0,
      engagementScore: this.calculateEngagementScore(data),
    };

    this.interactionQueue.push(interactionData);
    this.updateUserPreferences(interactionData);

    // Check storage size and flush if needed
    this.checkStorageSize();
  }

  // Calculate engagement score based on interaction data
  private calculateEngagementScore(data: Partial<VideoInteractionData>): number {
    let score = 0;

    // View duration (max 40 points)
    score += Math.min(data.viewDuration || 0, 300) / 300 * 40;

    // Interaction count (max 20 points)
    score += Math.min(data.interactionCount || 0, 10) / 10 * 20;

    // Completion rate (max 20 points)
    score += (data.completionRate || 0) * 20;

    // Viewport time ratio (max 10 points)
    const viewportRatio = data.viewportTime && data.viewDuration
      ? data.viewportTime / data.viewDuration
      : 0;
    score += Math.min(viewportRatio, 1) * 10;

    // Play count (max 10 points)
    score += Math.min(data.playCount || 0, 5) / 5 * 10;

    return Math.min(score, 100);
  }

  // Update user preferences based on interaction data
  private updateUserPreferences(data: VideoInteractionData): void {
    // Update preferred categories
    if (data.activityId && data.activityId !== 'unknown') {
      const category = this.extractCategoryFromActivityId(data.activityId);
      if (category && !this.preferences.preferredCategories.includes(category)) {
        this.preferences.preferredCategories.push(category);
        if (this.preferences.preferredCategories.length > 10) {
          this.preferences.preferredCategories = this.preferences.preferredCategories.slice(-10);
        }
      }
    }

    // Update autoplay preference
    if (data.playCount > 0) {
      this.preferences.autoplayPreference = true;
    }

    // Update mute preference
    if (data.muteCount > data.unmuteCount) {
      this.preferences.mutePreference = true;
    } else if (data.unmuteCount > data.muteCount) {
      this.preferences.mutePreference = false;
    }

    // Update interaction style
    if (data.interactionCount > 5) {
      this.preferences.interactionStyle = 'engaged';
    } else if (data.interactionCount > 2) {
      this.preferences.interactionStyle = 'active';
    } else {
      this.preferences.interactionStyle = 'passive';
    }

    // Update scroll speed
    if (data.scrollSpeed > 0) {
      this.preferences.scrollSpeed = data.scrollSpeed;
    }

    this.saveUserPreferences();
  }

  // Extract category from activity ID (mock implementation)
  private extractCategoryFromActivityId(activityId: string): string | null {
    // This would typically come from your activity data
    // For now, return a mock category based on activity ID
    const mockCategories = ['fitness', 'cooking', 'art', 'music', 'education'];
    const hash = activityId.split('').reduce((a, b) => {
      a = ((a << 5) - a) + b.charCodeAt(0);
      return a & a;
    }, 0);
    return mockCategories[Math.abs(hash) % mockCategories.length];
  }

  // Check storage size and flush if needed
  private checkStorageSize(): void {
    try {
      const storageSize = JSON.stringify(localStorage).length;
      const maxSizeBytes = this.config.maxStorageSize * 1024;

      if (storageSize > maxSizeBytes) {
        this.flush();
      }
    } catch (e) {
      console.warn('Failed to check storage size:', e);
    }
  }

  // Start auto-flush timer
  private startAutoFlush(): void {
    this.flushTimer = setInterval(() => {
      if (this.interactionQueue.length > 0) {
        this.flush();
      }
    }, this.config.flushInterval * 1000);
  }

  // Flush interaction data to analytics endpoint
  public flush(): void {
    if (this.interactionQueue.length === 0) return;

    const dataToSend = [...this.interactionQueue];
    this.interactionQueue = [];

    // In a real implementation, send to your analytics service
    console.log('📊 Flushing video analytics data:', dataToSend);

    // Example: Send to analytics service
    // fetch('/api/analytics/video-interactions', {
    //   method: 'POST',
    //   headers: { 'Content-Type': 'application/json' },
    //   body: JSON.stringify({ interactions: dataToSend })
    // }).catch(error => console.error('Failed to flush analytics:', error));

    // For now, store in localStorage as backup
    this.storeInLocalStorage(dataToSend);
  }

  // Store data in localStorage as backup
  private storeInLocalStorage(data: VideoInteractionData[]): void {
    try {
      const existing = localStorage.getItem('funny_activities_analytics_backup');
      const existingData = existing ? JSON.parse(existing) : [];
      const combinedData = [...existingData, ...data];

      // Keep only last 1000 entries to prevent storage bloat
      const trimmedData = combinedData.slice(-1000);

      localStorage.setItem('funny_activities_analytics_backup', JSON.stringify(trimmedData));
    } catch (e) {
      console.warn('Failed to store analytics backup:', e);
    }
  }

  // Get user preferences
  public getUserPreferences(): UserPreferences {
    return { ...this.preferences };
  }

  // Get analytics summary
  public getAnalyticsSummary(): {
    totalInteractions: number;
    averageEngagementScore: number;
    topCategories: string[];
    userEngagementLevel: string;
  } {
    const backupData = localStorage.getItem('funny_activities_analytics_backup');
    const interactions = backupData ? JSON.parse(backupData) : [];

    const totalInteractions = interactions.length;
    const averageEngagementScore = interactions.length > 0
      ? (interactions as VideoInteractionData[]).reduce((sum: number, i: VideoInteractionData) => sum + i.engagementScore, 0) / interactions.length
      : 0;

    const categoryCount: Record<string, number> = {};
    interactions.forEach((interaction: VideoInteractionData) => {
      const category = this.extractCategoryFromActivityId(interaction.activityId);
      if (category) {
        categoryCount[category] = (categoryCount[category] || 0) + 1;
      }
    });

    const topCategories = Object.entries(categoryCount)
      .sort(([,a], [,b]) => b - a)
      .slice(0, 5)
      .map(([category]) => category);

    let userEngagementLevel = 'low';
    if (averageEngagementScore > 60) userEngagementLevel = 'high';
    else if (averageEngagementScore > 30) userEngagementLevel = 'medium';

    return {
      totalInteractions,
      averageEngagementScore,
      topCategories,
      userEngagementLevel,
    };
  }

  // Clean up resources
  public destroy(): void {
    if (this.flushTimer) {
      clearInterval(this.flushTimer);
      this.flushTimer = null;
    }
    this.flush(); // Flush any remaining data
  }
}

// Create singleton instance
const videoAnalyticsService = new VideoAnalyticsService();

// Export for use in components
export default videoAnalyticsService;
export type { VideoInteractionData, UserPreferences, VideoAnalyticsConfig };