import { ComponentFixture } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

export class TestUtils {
  /**
   * Get element by CSS selector
   */
  static getElement<T = HTMLElement>(
    fixture: ComponentFixture<any>,
    selector: string
  ): T {
    return fixture.debugElement.query(By.css(selector))?.nativeElement;
  }

  /**
   * Get all elements by CSS selector
   */
  static getElements<T = HTMLElement>(
    fixture: ComponentFixture<any>,
    selector: string
  ): T[] {
    return fixture.debugElement.queryAll(By.css(selector)).map(el => el.nativeElement);
  }

  /**
   * Get element by test ID
   */
  static getElementByTestId<T = HTMLElement>(
    fixture: ComponentFixture<any>,
    testId: string
  ): T {
    return fixture.debugElement.query(By.css(`[data-testid="${testId}"]`))?.nativeElement;
  }

  /**
   * Get debug element by CSS selector
   */
  static getDebugElement(
    fixture: ComponentFixture<any>,
    selector: string
  ): DebugElement {
    return fixture.debugElement.query(By.css(selector));
  }

  /**
   * Trigger click event on element
   */
  static clickElement(fixture: ComponentFixture<any>, selector: string): void {
    const element = this.getElement(fixture, selector);
    if (element) {
      element.click();
      fixture.detectChanges();
    }
  }

  /**
   * Set input value
   */
  static setInputValue(
    fixture: ComponentFixture<any>,
    selector: string,
    value: string
  ): void {
    const input = this.getElement<HTMLInputElement>(fixture, selector);
    if (input) {
      input.value = value;
      input.dispatchEvent(new Event('input'));
      fixture.detectChanges();
    }
  }

  /**
   * Check if element has class
   */
  static hasClass(
    fixture: ComponentFixture<any>,
    selector: string,
    className: string
  ): boolean {
    const element = this.getElement(fixture, selector);
    return element?.classList.contains(className) || false;
  }

  /**
   * Check if element has attribute
   */
  static hasAttribute(
    fixture: ComponentFixture<any>,
    selector: string,
    attribute: string,
    value?: string
  ): boolean {
    const element = this.getElement(fixture, selector);
    if (!element) return false;
    
    if (value) {
      return element.getAttribute(attribute) === value;
    }
    return element.hasAttribute(attribute);
  }

  /**
   * Get text content of element
   */
  static getTextContent(
    fixture: ComponentFixture<any>,
    selector: string
  ): string {
    const element = this.getElement(fixture, selector);
    return element?.textContent?.trim() || '';
  }

  /**
   * Check if element is visible
   */
  static isVisible(
    fixture: ComponentFixture<any>,
    selector: string
  ): boolean {
    const element = this.getElement(fixture, selector);
    if (!element) return false;
    
    const style = window.getComputedStyle(element);
    return style.display !== 'none' && 
           style.visibility !== 'hidden' && 
           element.offsetWidth > 0 && 
           element.offsetHeight > 0;
  }

  /**
   * Wait for async operations to complete
   */
  static async waitForAsync(fixture: ComponentFixture<any>): Promise<void> {
    fixture.detectChanges();
    await fixture.whenStable();
  }

  /**
   * Create a mock event
   */
  static createMockEvent(type: string, options: any = {}): Event {
    return new Event(type, { bubbles: true, cancelable: true, ...options });
  }

  /**
   * Create a mock keyboard event
   */
  static createMockKeyboardEvent(
    type: string,
    key: string,
    options: any = {}
  ): KeyboardEvent {
    return new KeyboardEvent(type, {
      key,
      bubbles: true,
      cancelable: true,
      ...options
    });
  }

  /**
   * Create a mock mouse event
   */
  static createMockMouseEvent(
    type: string,
    options: any = {}
  ): MouseEvent {
    return new MouseEvent(type, {
      bubbles: true,
      cancelable: true,
      ...options
    });
  }

  /**
   * Dispatch event on element
   */
  static dispatchEvent(
    fixture: ComponentFixture<any>,
    selector: string,
    event: Event
  ): void {
    const element = this.getElement(fixture, selector);
    if (element) {
      element.dispatchEvent(event);
      fixture.detectChanges();
    }
  }

  /**
   * Check if element contains text
   */
  static containsText(
    fixture: ComponentFixture<any>,
    selector: string,
    text: string
  ): boolean {
    const element = this.getElement(fixture, selector);
    return element?.textContent?.includes(text) || false;
  }

  /**
   * Get computed style property
   */
  static getComputedStyle(
    fixture: ComponentFixture<any>,
    selector: string,
    property: string
  ): string {
    const element = this.getElement(fixture, selector);
    if (!element) return '';
    
    return window.getComputedStyle(element).getPropertyValue(property);
  }

  /**
   * Check if element is disabled
   */
  static isDisabled(
    fixture: ComponentFixture<any>,
    selector: string
  ): boolean {
    const element = this.getElement<HTMLElement>(fixture, selector);
    return element?.hasAttribute('disabled') || false;
  }

  /**
   * Check if element is focused
   */
  static isFocused(
    fixture: ComponentFixture<any>,
    selector: string
  ): boolean {
    const element = this.getElement<HTMLElement>(fixture, selector);
    return document.activeElement === element;
  }
} 